using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Extensions;
using RunSuggestion.Api.Helpers;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Api.Functions;

public class GetRunRecommendation
{
    private readonly ILogger<GetRunRecommendation> _logger;
    private readonly IAuthenticator _authenticator;
    private readonly IRecommendationService _recommendationService;

    public GetRunRecommendation(ILogger<GetRunRecommendation> logger, IAuthenticator authenticator,
        IRecommendationService recommendationService)
    {
        _logger = logger;
        _authenticator = authenticator;
        _recommendationService = recommendationService;
    }

    [Function(nameof(GetRunRecommendation))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest request)
    {
        _logger.LogInformation(Messages.Recommendation.RequestReceived);

        string? entraId = _authenticator.Authenticate(request.GetAuthHeader());

        if (entraId is null)
        {
            _logger.LogWarning(Messages.Authentication.Failure);
            return new UnauthorizedResult();
        }

        _logger.LogInformation("{AuthSuccessMessage}: ...{entraId}",
                               Messages.Authentication.Success,
                               AuthHelpers.GetLastFiveChars(entraId));

        RunRecommendation _ = await _recommendationService.GetRecommendation(entraId);

        return new OkObjectResult(string.Empty);
    }
}
