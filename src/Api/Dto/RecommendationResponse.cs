using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Api.Dto;

public record RecommendationResponse
{
    /// <summary>
    /// The RunRecommendation that the service has chosen for the current user based on
    /// the existing history
    /// </summary>
    public required RunRecommendation Recommendation { get; init; }
}
