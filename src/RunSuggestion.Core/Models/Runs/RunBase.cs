namespace RunSuggestion.Core.Models.Runs;

/// <summary>
/// Base class used to reduce repetition between internal run models
/// </summary>
public class RunBase
{
    /// <summary>
    /// The serial for the ID within the database
    /// </summary>
    public int RunEventId { get; init; }

    /// <summary>
    /// The date that the run took place
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// The overall distance (in metres) of the run
    /// </summary>
    public int Distance { get; init; }

    /// <summary>
    /// Essentially the rate of perceived exhertion (RPE)
    /// How difficult the run feels on a scale of 1-10
    /// </summary>
    public byte Effort { get; init; }

    /// <summary>
    /// How long the run took, as a C# TimeSpan.
    /// TimeSpans allow the duration to be expressed easily in any appropriate time measurement
    /// </summary>
    /// <example>
    /// // Display as "45 minutes"
    /// $"{Duration.TotalMinutes:F0} minutes"
    /// </example>
    public TimeSpan Duration { get; init; }
}
