using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Api.Dto;
using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Api.Functions;

public class PostRunHistory
{
    private const string LogMessageUploadStarted = "Run history upload started.";
    private const string LogMessageAuthenticationFailure = "Failed to authenticate user.";
    private const string LogMessageAuthenticationSuccess = "Successfully Authenticated user";

    private const string AuthorizationHeader = "Authorization";


    private readonly ILogger<PostRunHistory> _logger;
    private readonly IAuthenticator _authenticator;
    private readonly IRunHistoryAdder _runHistoryAdder;

    public PostRunHistory(ILogger<PostRunHistory> logger, IAuthenticator authenticator,
        IRunHistoryAdder runHistoryAdder)
    {
        _logger = logger;
        _authenticator = authenticator;
        _runHistoryAdder = runHistoryAdder;
    }

    [Function("PostRunHistory")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        _logger.LogInformation(LogMessageUploadStarted);

        string authHeader = request.Headers[AuthorizationHeader].ToString();
        string? entraId = _authenticator.Authenticate(authHeader);

        if (entraId is null)
        {
            _logger.LogWarning(LogMessageAuthenticationFailure);
            return new UnauthorizedResult();
        }

        _logger.LogInformation("{AuthSuccessMessage}: ...{entraId}",
                               LogMessageAuthenticationSuccess,
                               GetLastFiveChars(entraId));

        using StreamReader reader = new(request.Body);
        string csv = await reader.ReadToEndAsync();

        int affectedRows = await _runHistoryAdder.AddRunHistory(entraId, csv);
        UploadResponse response = new()
        {
            Message = "Upload Successful",
            RowsAdded = affectedRows
        };
        return new OkObjectResult(response);
    }

    /// <summary>
    /// Returns the last 5 characters of the passed string
    /// </summary>
    /// <param name="fullString"></param>
    /// <returns>the last 5 characters of the passed string</returns>
    private static string GetLastFiveChars(string fullString) => fullString[^5..];
}
