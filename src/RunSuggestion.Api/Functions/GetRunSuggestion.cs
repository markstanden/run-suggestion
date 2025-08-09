using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Helpers;
using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Api.Functions;

public class GetRunSuggestion
{
    private const string LogRequestReceived = "Run suggestion request received.";
    private const string MessageAuthenticationSuccess = "Successfully Authenticated user";
    private readonly ILogger<GetRunSuggestion> _logger;
    private readonly IAuthenticator _authenticator;

    public GetRunSuggestion(ILogger<GetRunSuggestion> logger, IAuthenticator authenticator)
    {
        _logger = logger;
        _authenticator = authenticator;
    }

    [Function("GetRunSuggestion")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request)
    {
        _logger.LogInformation(LogRequestReceived);

        string authHeader = request.Headers[Headers.Authorization].ToString();
        string? entraId = _authenticator.Authenticate(authHeader);

        _logger.LogInformation("{AuthSuccessMessage}: ...{entraId}",
                               MessageAuthenticationSuccess,
                               AuthHelpers.GetLastFiveChars(entraId));

        return new OkObjectResult(string.Empty);
    }
}
