using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Core.Models.Users;

public class UserData
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public int UserId { get; init; }

    /// <summary>
    /// Unique authentication GUID for the user.
    /// </summary>
    public string? EntraId { get; set; }

    /// <summary>
    /// The current run history of the user.
    /// Used to calculate current running load and weekly averages
    /// </summary>
    public IEnumerable<RunEvent> RunHistory { get; init; } = [];

    /// <summary>
    /// The current run recommendation history of the user.
    /// Used to record previous recommendations
    /// </summary>
    public IEnumerable<RunEvent> RunRecommendationHistory { get; init; } = [];
}
