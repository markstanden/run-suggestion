namespace RunSuggestion.Core.Models.Runs;

public record RunEvent : RunBase
{
    /// <summary>
    /// The serial for the ID within the database
    /// </summary>
    public int RunEventId { get; init; }
}
