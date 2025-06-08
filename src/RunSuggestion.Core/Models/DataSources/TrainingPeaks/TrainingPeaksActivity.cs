namespace RunSuggestion.Core.Models.DataSources.TrainingPeaks;

public class TrainingPeaksActivity
{
    /// <summary>
    /// In my sample data this is the activity type as reported by Garmin
    /// <example>"Running"</example>
    /// <example>"Cardio"</example>
    /// <example>"Yoga"</example>
    /// </summary>
    public string Title { get; init; } = string.Empty;

    public DateTime WorkoutDay { get; init; }
    public double DistanceInMeters { get; init; }
    public double TimeTotalInHours { get; init; }
    public int? HeartRateAverage { get; init; }
    public int? HeartRateMax { get; init; }
    public int? Rpe { get; init; }
    public int? Feeling { get; init; }
}
