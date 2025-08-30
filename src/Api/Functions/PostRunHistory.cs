using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Extensions;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Extensions;
using RunSuggestion.Shared.Models.Dto;

namespace RunSuggestion.Api.Functions;

public class PostRunHistory(
    ILogger<PostRunHistory> logger,
    IAuthenticator authenticator,
    IRunHistoryAdder runHistoryAdder)
{
    private readonly ILogger<PostRunHistory> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IAuthenticator _authenticator =
        authenticator ?? throw new ArgumentNullException(nameof(authenticator));

    private readonly IRunHistoryAdder _runHistoryAdder =
        runHistoryAdder ?? throw new ArgumentNullException(nameof(runHistoryAdder));

    [Function(nameof(PostRunHistory))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = Routes.UploadPath)]
        HttpRequest request)
    {
        _logger.LogInformation(Messages.CsvUpload.RequestReceived);

        string? entraId = _authenticator.Authenticate(request.GetAuthHeader());

        if (entraId is null)
        {
            _logger.LogWarning(Messages.Authentication.Failure);
            return new UnauthorizedResult();
        }

        _logger.LogInformation("{AuthSuccessMessage}: ...{EntraId}",
                               Messages.Authentication.Success,
                               entraId.LastFiveChars());

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
