using RunSuggestion.Shared.Models.Runs;

namespace RunSuggestion.Core.Interfaces;

public interface IRecommendationService
{
    /// <summary>
    /// Creates a run recommendation based on the users' preferences and the recent run history.
    /// </summary>
    /// <param name="entraId">The User's Unique identifier within the authentication provider</param>
    /// <returns>A run recommendation based on the users' preferences and run history</returns>
    Task<RunRecommendation> GetRecommendationAsync(string entraId);
}
