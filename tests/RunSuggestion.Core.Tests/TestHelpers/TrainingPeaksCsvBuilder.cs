using System.Globalization;
using RunSuggestion.Core.Models.Runs.DataSources.TrainingPeaks;

namespace RunSuggestion.Core.Tests.TestHelpers;

public class TrainingPeaksCsvBuilder
{
    public const string RunningTitle = "Running";
    public const string RunningWorkoutType = "Run";

    public const string DefaultTitle = RunningTitle;
    public const string DefaultWorkoutType = RunningWorkoutType;
    public const string DefaultWorkoutDay = "2024-12-31";
    public const double DefaultDistanceInMeters = 5000;
    public const double DefaultTotalTimeInHours = 0.5;
    public const int DefaultHeartRateAverage = 100;
    public const int DefaultHeartRateMax = 200;
    public const int DefaultRpe = 3;
    public const int DefaultFeeling = 5;

    /// <summary>
    /// Internal class field to store the CSV rows added to the string output by the Build() method
    /// </summary>
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
        string title = DefaultTitle,
        string workoutType = DefaultWorkoutType,
        string workoutDay = DefaultWorkoutDay,
        double distanceInMeters = DefaultDistanceInMeters,
        double totalTimeInHours = DefaultTotalTimeInHours,
        int heartRateAverage = DefaultHeartRateAverage,
        int heartRateMax = DefaultHeartRateMax,
        int rpe = DefaultRpe,
        int feeling = DefaultFeeling
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
        string workoutDay = DefaultWorkoutDay,
        double distanceInMeters = DefaultDistanceInMeters,
        double totalTimeInHours = DefaultTotalTimeInHours,
        int heartRateAverage = DefaultHeartRateAverage,
        int heartRateMax = DefaultHeartRateMax,
        int rpe = DefaultRpe,
        int feeling = DefaultFeeling
    )
    {
        return AddRow(
            title: RunningTitle,
            workoutType: RunningWorkoutType,
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
