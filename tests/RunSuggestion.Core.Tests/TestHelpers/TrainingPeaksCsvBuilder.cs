using System.Globalization;
using RunSuggestion.Core.Models.Runs.DataSources.TrainingPeaks;

namespace RunSuggestion.Core.Tests.TestHelpers;

public class TrainingPeaksCsvBuilder
{
    public const string RUNNING_TITLE = "Running";
    public const string RUNNING_WORKOUT_TYPE = "Run";

    public const string DEFAULT_TITLE = RUNNING_TITLE;
    public const string DEFAULT_WORKOUT_TYPE = RUNNING_WORKOUT_TYPE;
    public const string DEFAULT_WORKOUT_DAY = "2024-12-31";
    public const double DEFAULT_DISTANCE_IN_METERS = 5000;
    public const double DEFAULT_TOTAL_TIME_IN_HOURS = 0.5;
    public const int DEFAULT_HEART_RATE_AVERAGE = 100;
    public const int DEFAULT_HEART_RATE_MAX = 200;
    public const int DEFAULT_RPE = 3;
    public const int DEFAULT_FEELING = 5;

    private IEnumerable<TrainingPeaksActivity> _rows = new List<TrainingPeaksActivity>();

    /// <summary>
    /// Builds the CSV file and returns as a string
    /// </summary>
    /// <returns>a csv string containing the TrainingPeaks headers with any added rows on newlines</returns>
    public string Build()
    {
        IEnumerable<string> csvRows = _rows.Select(row => CreateRow(row));
        return $"{CreateHeaders()}\n{string.Join('\n', csvRows)}";
    }

    /// <summary>
    /// Adds a TrainingPeaksActivity to the csv being built
    /// </summary>
    /// <param name="activity">The TrainingPeaksActivity to add</param>
    /// <returns>The current builder instance for method chaining</returns>
    public TrainingPeaksCsvBuilder AddRow(TrainingPeaksActivity activity)
    {
        _rows = _rows.Append(activity);
        return this;
    }

    /// <summary>
    /// Overload to add a TrainingPeaksActivity by individual parameters to the csv being built
    /// default values are set for all params, allowing individual fields to be overridden and tested,
    /// without needed to specify unnecessary (for the test) values.
    /// </summary>
    /// <param name="title">The activity title (e.g., "Running", "Cardio")</param>
    /// <param name="workoutType">The workout type subcategory (e.g., "Run", "Other")</param>
    /// <param name="workoutDay">The workout date in the TrainingPeaks default (yyyy-MM-dd) format</param>
    /// <param name="distanceInMeters">Total distance in metres</param>
    /// <param name="totalTimeInHours">Total duration in fractional hours</param>
    /// <param name="heartRateAverage">Average heart rate during activity</param>
    /// <param name="heartRateMax">Maximum heart rate during activity</param>
    /// <param name="rpe">Rate of perceived exertion (RPE) (expected values 1-10 where ten is maximum effort)</param>
    /// <param name="feeling">How the user felt (1-5 garmin emoji scale)</param>
    /// <returns>The current builder instance for method chaining</returns>
    public TrainingPeaksCsvBuilder AddRow(
        string title = DEFAULT_TITLE,
        string workoutType = DEFAULT_WORKOUT_TYPE,
        string workoutDay = DEFAULT_WORKOUT_DAY,
        double distanceInMeters = DEFAULT_DISTANCE_IN_METERS,
        double totalTimeInHours = DEFAULT_TOTAL_TIME_IN_HOURS,
        int heartRateAverage = DEFAULT_HEART_RATE_AVERAGE,
        int heartRateMax = DEFAULT_HEART_RATE_MAX,
        int rpe = DEFAULT_RPE,
        int feeling = DEFAULT_FEELING
    )
    {
        TrainingPeaksActivity activity = new()
        {
            Title = title,
            WorkoutType = workoutType,
            WorkoutDay = ParseWorkoutDay(workoutDay),
            DistanceInMeters = distanceInMeters,
            TimeTotalInHours = totalTimeInHours,
            HeartRateAverage = heartRateAverage,
            HeartRateMax = heartRateMax,
            Rpe = rpe,
            Feeling = feeling
        };
        return AddRow(activity);
    }

    /// <summary>
    /// Further convenience method to add a running specific row, with the
    /// workout title and workoutType set to Running specific values.
    /// Other values are set to defaults by default
    /// </summary>
    /// <param name="workoutDay">The workout date in the TrainingPeaks default (yyyy-MM-dd) format</param>
    /// <param name="distanceInMeters">Total distance in metres</param>
    /// <param name="totalTimeInHours">Total duration in fractional hours</param>
    /// <param name="heartRateAverage">Average heart rate during activity</param>
    /// <param name="heartRateMax">Maximum heart rate during activity</param>
    /// <param name="rpe">Rate of perceived exertion (RPE) (expected values 1-10 where ten is maximum effort)</param>
    /// <param name="feeling">How the user felt (1-5 garmin emoji scale)</param>
    /// <returns>The current builder instance for method chaining</returns>
    public TrainingPeaksCsvBuilder AddRunningRow(
        string workoutDay = DEFAULT_WORKOUT_DAY,
        double distanceInMeters = DEFAULT_DISTANCE_IN_METERS,
        double totalTimeInHours = DEFAULT_TOTAL_TIME_IN_HOURS,
        int heartRateAverage = DEFAULT_HEART_RATE_AVERAGE,
        int heartRateMax = DEFAULT_HEART_RATE_MAX,
        int rpe = DEFAULT_RPE,
        int feeling = DEFAULT_FEELING
    )
    {
        return AddRow(
            title: RUNNING_TITLE,
            workoutType: RUNNING_WORKOUT_TYPE,
            workoutDay: workoutDay,
            distanceInMeters: distanceInMeters,
            totalTimeInHours: totalTimeInHours,
            heartRateAverage: heartRateAverage,
            heartRateMax: heartRateMax,
            rpe: rpe,
            feeling: feeling
        );
    }

    /// <summary>
    /// Private convenience method to create the required TrainingPeaks CSV headers
    /// </summary>
    /// <returns>a string containing the headers only</returns>
    private static string CreateHeaders() => $"\"{nameof(TrainingPeaksActivity.Title)}\",\"{nameof(TrainingPeaksActivity.WorkoutType)}\",\"{nameof(TrainingPeaksActivity.WorkoutDay)}\",\"{nameof(TrainingPeaksActivity.DistanceInMeters)}\",\"{nameof(TrainingPeaksActivity.TimeTotalInHours)}\",\"{nameof(TrainingPeaksActivity.HeartRateAverage)}\",\"{nameof(TrainingPeaksActivity.HeartRateMax)}\",\"{nameof(TrainingPeaksActivity.Rpe)}\",\"{nameof(TrainingPeaksActivity.Feeling)}\"";


    /// <summary>
    /// Private convenience method to convert a training peaks row into a CSV string
    /// column headers are provided in the expected order
    /// </summary>
    /// <param name="row">The TrainingPeaksActivity to convert into a CSV</param>
    /// <returns>the converted row as a csv</returns>
    private static string CreateRow(TrainingPeaksActivity row) =>
        $"\"{row.Title}\",\"{row.WorkoutType}\",\"{row.WorkoutDay.ToString(TrainingPeaksActivity.WORKOUT_DAY_DATETIME_FORMAT)}\",\"{row.DistanceInMeters}\",\"{row.TimeTotalInHours}\",\"{row.HeartRateAverage}\",\"{row.HeartRateMax}\",\"{row.Rpe}\",\"{row.Feeling}\"";


    /// <summary>
    /// Parses a workout day date string into a DateTime object.
    /// </summary>
    /// <param name="workoutDay">The date string to parse, default format is TrainingPeaks default (yyyy-MM-dd)</param>
    /// <param name="format">the format of the date string - defaults to use TrainingPeaks default (yyyy-MM-dd)</param>
    /// <returns>DateTime representation of the passed string</returns>
    public static DateTime ParseWorkoutDay(string workoutDay, string format = TrainingPeaksActivity.WORKOUT_DAY_DATETIME_FORMAT)
    {
        return DateTime.ParseExact(
            workoutDay,
            format,
            CultureInfo.InvariantCulture);
    }
}
