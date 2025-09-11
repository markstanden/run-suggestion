namespace RunSuggestion.Shared.Models.Runs;

public record RunRecommendation : RunBase
{
    /// <summary>
    /// The serial for the ID within the database
    /// </summary>
    public int? RunRecommendationId { get; init; }
}
