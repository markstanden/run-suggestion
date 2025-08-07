namespace RunSuggestion.Core.Models.Runs;

public record RunSuggestion : RunBase
{
    /// <summary>
    /// The serial for the ID within the database
    /// </summary>
    public int RunSuggestionId { get; init; }
};
