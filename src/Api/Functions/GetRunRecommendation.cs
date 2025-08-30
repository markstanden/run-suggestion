using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Extensions;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Extensions;
using RunSuggestion.Shared.Models.Dto;

namespace RunSuggestion.Api.Functions;

public class GetRunRecommendation(
    ILogger<GetRunRecommendation> logger,
    IAuthenticator authenticator,
    IRecommendationService recommendationService)
{
    private readonly ILogger<GetRunRecommendation> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IAuthenticator _authenticator =
        authenticator ?? throw new ArgumentNullException(nameof(authenticator));

    private readonly IRecommendationService _recommendationService =
        recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));

    [Function(nameof(GetRunRecommendation))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "recommendations")]
        HttpRequest request)
    {
        _logger.LogInformation(Messages.Recommendation.RequestReceived);

        string? entraId = _authenticator.Authenticate(request.GetAuthHeader());

        if (entraId is null)
        {
            _logger.LogWarning(Messages.Authentication.Failure);
            return new UnauthorizedResult();
        }

        _logger.LogInformation("{AuthSuccessMessage}: ...{EntraId}",
                               Messages.Authentication.Success,
                               entraId.LastFiveChars());

        RecommendationResponse response = new()
        {
            Recommendation = await _recommendationService.GetRecommendation(entraId)
        };

        return new OkObjectResult(response);
    }
}
