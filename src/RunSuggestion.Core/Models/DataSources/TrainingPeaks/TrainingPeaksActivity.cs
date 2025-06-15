namespace RunSuggestion.Core.Models.DataSources.TrainingPeaks;

public class TrainingPeaksActivity
{
    public const string WORKOUT_DAY_DATETIME_FORMAT = "yyyy-MM-dd";
    public const string RUNNING_ACTIVITY_TITLE = "Running";

    /// <summary>
    /// In my sample data this is the activity type as reported by Garmin
    /// <example>"Running"</example>
    /// <example>"Cardio"</example>
    /// <example>"Yoga"</example>
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// In my sample data this looks like a subcategory of the main activity type
    /// <example>"Run"</example>
    /// <example>"Other"</example>
    /// </summary>
    public string WorkoutType { get; init; } = string.Empty;

    public DateTime WorkoutDay { get; init ; }
    public double DistanceInMeters { get; init; }
    public double TimeTotalInHours { get; init; }
    public int? HeartRateAverage { get; init; }
    public int? HeartRateMax { get; init; }
    public int? Rpe { get; init; }
    public int? Feeling { get; init; }
}
