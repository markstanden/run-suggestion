using System.Net.Http.Json;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Dto;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.Web.Constants;
using RunSuggestion.Web.Interfaces;

namespace RunSuggestion.Web.Services;

public class RecommendationApiService(ILogger<RecommendationApiService> logger, HttpClient httpClient)
    : IRecommendationApiService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    private readonly ILogger<RecommendationApiService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<RunRecommendation?> GetRecommendationAsync()
    {
        _logger.LogInformation(Logs.Recommendation.Start);

        HttpRequestMessage request = new(HttpMethod.Get, Routes.RecommendationPath);
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(Logs.Recommendation.Success);
            RecommendationResponse? recommendationResponse =
                await response.Content.ReadFromJsonAsync<RecommendationResponse>();
            return recommendationResponse?.Recommendation ?? null;
        }
        else
        {
            _logger.LogWarning(Logs.Recommendation.Failure);
            return null;
        }
    }
}
