using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Helpers;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Api.Functions;

public class GetRunSuggestion
{
    private const string RunSuggestionFunctionName = "GetRunSuggestion";


    private readonly ILogger<GetRunSuggestion> _logger;
    private readonly IAuthenticator _authenticator;
    private readonly IRecommendationService _recommendationService;

    public GetRunSuggestion(ILogger<GetRunSuggestion> logger, IAuthenticator authenticator,
        IRecommendationService recommendationService)
    {
        _logger = logger;
        _authenticator = authenticator;
        _recommendationService = recommendationService;
    }

    [Function(RunSuggestionFunctionName)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request)
    {
        _logger.LogInformation(LogMessages.RequestReceived);

        string authHeader = request.Headers[Headers.Authorization].ToString();
        string? entraId = _authenticator.Authenticate(authHeader);

        if (entraId is null)
        {
            _logger.LogWarning(LogMessages.Authentication.Failure);
            return new UnauthorizedResult();
        }

        _logger.LogInformation("{AuthSuccessMessage}: ...{entraId}",
                               LogMessages.Authentication.Success,
                               AuthHelpers.GetLastFiveChars(entraId));

        RunRecommendation _ = await _recommendationService.GetRecommendation(entraId);

        return new OkObjectResult(string.Empty);
    }
}
