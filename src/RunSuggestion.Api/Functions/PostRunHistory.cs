using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Api.Dto;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Extensions;
using RunSuggestion.Api.Helpers;

namespace RunSuggestion.Api.Functions;

public class PostRunHistory
{
    private readonly ILogger<PostRunHistory> _logger;
    private readonly IAuthenticator _authenticator;
    private readonly IRunHistoryAdder _runHistoryAdder;

    public PostRunHistory(ILogger<PostRunHistory> logger, IAuthenticator authenticator,
        IRunHistoryAdder runHistoryAdder)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
        _runHistoryAdder = runHistoryAdder ?? throw new ArgumentNullException(nameof(runHistoryAdder));
    }

    [Function(nameof(PostRunHistory))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        _logger.LogInformation(Messages.CsvUpload.RequestReceived);

        string? entraId = _authenticator.Authenticate(request.GetAuthHeader());

        if (entraId is null)
        {
            _logger.LogWarning(Messages.Authentication.Failure);
            return new UnauthorizedResult();
        }

        _logger.LogInformation("{AuthSuccessMessage}: ...{entraId}",
                               Messages.Authentication.Success,
                               AuthHelpers.GetLastFiveChars(entraId));

        using StreamReader reader = new(request.Body);
        string csv = await reader.ReadToEndAsync();

        try
        {
            int affectedRows = await _runHistoryAdder.AddRunHistory(entraId, csv);
            UploadResponse response = new()
            {
                Message = Messages.CsvUpload.Success,
                RowsAdded = affectedRows
            };
            return new OkObjectResult(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex,
                               "{FailureMessage}: {InvalidCsv} - {ExceptionMessage}",
                               Messages.CsvUpload.Failure,
                               Messages.CsvUpload.Invalid,
                               ex.Message);
            UploadResponse errorResponse = new()
            {
                Message = $"{Messages.CsvUpload.Failure}: {Messages.CsvUpload.Invalid} - {ex.Message}",
                RowsAdded = 0
            };
            return new BadRequestObjectResult(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                             "{FailureMessage}: {ExceptionMessage}",
                             Messages.CsvUpload.Failure,
                             ex.Message);
            UploadResponse errorResponse = new()
            {
                Message = Messages.UnexpectedError,
                RowsAdded = 0
            };
            return new ObjectResult(errorResponse)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
