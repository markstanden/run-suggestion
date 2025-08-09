using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Api.Dto;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Helpers;

namespace RunSuggestion.Api.Functions;

public class PostRunHistory
{
    private const string PostRunHistoryFunctionName = "PostRunHistory";

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

    [Function(PostRunHistoryFunctionName)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        _logger.LogInformation(Messages.Csv.UploadStarted);

        string authHeader = request.Headers[Headers.Authorization].ToString();
        string? entraId = _authenticator.Authenticate(authHeader);

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
                Message = Messages.Csv.Success,
                RowsAdded = affectedRows
            };
            return new OkObjectResult(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex,
                               "{FailureMessage}: {InvalidCsv} - {ExceptionMessage}",
                               Messages.Csv.Failure,
                               Messages.Csv.Invalid,
                               ex.Message);
            UploadResponse errorResponse = new()
            {
                Message = $"{Messages.Csv.Failure}: {Messages.Csv.Invalid} - {ex.Message}",
                RowsAdded = 0
            };
            return new BadRequestObjectResult(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                             "{FailureMessage}: {ExceptionMessage}",
                             Messages.Csv.Failure,
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
