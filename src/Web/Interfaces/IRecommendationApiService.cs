using RunSuggestion.Shared.Models.Dto;

namespace RunSuggestion.Web.Interfaces;

public interface IRecommendationApiService
{
    /// <summary>
    /// Service to request a run recommendation from the backend API
    /// </summary>
    /// <returns>
    /// The <see cref="RecommendationResponse">run recommendation response DTO</see>
    /// </returns>
    public Task<RecommendationResponse> GetRecommendationAsync();
}
