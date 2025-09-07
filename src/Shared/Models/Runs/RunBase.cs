namespace RunSuggestion.Shared.Models.Runs;

/// <summary>
/// Base class used to reduce repetition between internal run models
/// </summary>
public record RunBase
{
    /// <summary>
    /// The date that the run took place
    /// </summary>
    public DateTime Date { get; init; }

    /// <summary>
    /// The overall distance (in metres) of the run
    /// </summary>
    public int Distance { get; init; }

    /// <summary>
    /// Essentially the Rate of Perceived Exertion (RPE)
    /// How difficult the run feels on a scale of 1-10
    /// </summary>
    public byte Effort { get; init; }

    /// <summary>
    /// How long the run took, as a C# TimeSpan,
    /// allowing durations to be expressed in any appropriate time measurement
    /// </summary>
    /// <example>
    /// // Display as "45 minutes"
    /// $"{Duration.TotalMinutes:F0} minutes"
    /// </example>
    /// <seealso cref="TimeSpan">TimeSpan</seealso> 
    public TimeSpan Duration { get; init; }
}
