namespace RunSuggestion.Core.Models.DataSources.TrainingPeaks;

public class TrainingPeaksActivity
{
    /// <summary>
    /// The date format used within the TrainingPeaks data to be used by C# DateTime parser
    /// </summary>
    public const string WORKOUT_DAY_DATETIME_FORMAT = "yyyy-MM-dd";

    /// <summary>
    /// The activity title provided by TrainingPeaks for all running activities.
    /// </summary>
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

    /// <summary>
    /// The date that the activity was performed.
    /// TrainingPeaks provides the data in the following format: "yyyy-MM-dd"
    /// and it is parsed automatically into a DateTime.
    /// </summary>
    public DateTime WorkoutDay { get; init; }

    /// <summary>
    /// Total distance covered in metres.
    /// Unlike our internal model TrainingPeaks uses a double precision float to represent the distance,
    /// so this will be rounded to the nearest metre when used internally.
    /// </summary>
    public double DistanceInMeters { get; init; }

    /// <summary>
    /// Total duration of the activity in fractional hours.
    /// (e.g. 1.5 hours = 1 hour 30 mins)
    /// </summary>
    public double TimeTotalInHours { get; init; }

    /// <summary>
    /// Average heart rate recorded during the activity as a integer.
    /// set to null if not provided in the data.
    /// </summary>
    public int? HeartRateAverage { get; init; }

    /// <summary>
    /// Maximum heart rate recorded during the activity as a integer.
    /// set to null if not provided in the data.
    /// </summary>
    public int? HeartRateMax { get; init; }

    /// <summary>
    /// Rate of percieved exhertion as recorded in the Garmin Connect app/Training peaks portal.
    /// Recorded on a sliding scale between 1-10
    /// set to null if not provided by the user (requires event review in the Garmin Connect app).
    /// </summary>
    public int? Rpe { get; init; }

    /// <summary>
    /// 'How did you feel' score as recorded in the Garmin Connect app/Training peaks portal.
    /// Recorded on an emoji face score of 1-5 inclusive
    /// set to null if not provided by the user (requires event review in the Garmin Connect app).
    /// </summary>
    public int? Feeling { get; init; }

    /// <summary>
    /// Predicate that returns true only if the TrainingPeaks activity is a Run
    /// </summary>
    /// <param name="activity">Parsed line from Trainingpeaks CSV</param>
    /// <returns>True if the activity is a Run</returns>
    public static bool IsRunActivity(TrainingPeaksActivity activity) =>
        activity.Title == TrainingPeaksActivity.RUNNING_ACTIVITY_TITLE;
}
