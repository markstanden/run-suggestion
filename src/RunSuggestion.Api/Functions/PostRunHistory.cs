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
    private const string MessageUploadStarted = "Run history upload started.";
    private const string MessageAuthenticationFailure = "Failed to authenticate user.";
    private const string MessageAuthenticationSuccess = "Successfully Authenticated user";
    private const string MessageInvalidCsvContent = "Invalid CSV content";
    private const string MessageUnexpectedError = "An unexpected error occurred";
    private const string MessageSuccess = "Successfully processed CSV";
    private const string MessageFailure = "CSV Import Failed";

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

    [Function("PostRunHistory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        _logger.LogInformation(MessageUploadStarted);

        string authHeader = request.Headers[Headers.Authorization].ToString();
        string? entraId = _authenticator.Authenticate(authHeader);

        if (entraId is null)
        {
            _logger.LogWarning(MessageAuthenticationFailure);
            return new UnauthorizedResult();
        }

        _logger.LogInformation("{AuthSuccessMessage}: ...{entraId}",
                               MessageAuthenticationSuccess,
                               AuthHelpers.GetLastFiveChars(entraId));

        using StreamReader reader = new(request.Body);
        string csv = await reader.ReadToEndAsync();

        try
        {
            int affectedRows = await _runHistoryAdder.AddRunHistory(entraId, csv);
            UploadResponse response = new()
            {
                Message = MessageSuccess,
                RowsAdded = affectedRows
            };
            return new OkObjectResult(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex,
                               "{FailureMessage}: {InvalidCsv} - {ExceptionMessage}",
                               MessageFailure,
                               MessageInvalidCsvContent,
                               ex.Message);
            UploadResponse errorResponse = new()
            {
                Message = $"{MessageFailure}: {MessageInvalidCsvContent} - {ex.Message}",
                RowsAdded = 0
            };
            return new BadRequestObjectResult(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                             "{FailureMessage}: {ExceptionMessage}",
                             MessageFailure,
                             ex.Message);
            UploadResponse errorResponse = new()
            {
                Message = MessageUnexpectedError,
                RowsAdded = 0
            };
            return new ObjectResult(errorResponse)
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
