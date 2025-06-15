using System.Globalization;
using RunSuggestion.Core.Models.Runs.DataSources.TrainingPeaks;

namespace RunSuggestion.Core.Tests.TestHelpers;

public class TrainingPeaksCsvBuilder
{
    public const string DEFAULT_TITLE = "Running";
    public const string DEFAULT_WORKOUT_TYPE = "Run";
    public const string DEFAULT_WORKOUT_DAY = "2024-12-31";
    public const double DEFAULT_DISTANCE_IN_METERS = 5000;
    public const double DEFAULT_TOTAL_TIME_IN_HOURS = 0.5;
    public const int DEFAULT_HEART_RATE_AVERAGE = 100;
    public const int DEFAULT_HEART_RATE_MAX = 200;
    public const int DEFAULT_RPE = 3;
    public const int DEFAULT_FEELING = 5;

    private IEnumerable<TrainingPeaksActivity> _rows = new List<TrainingPeaksActivity>();

    public string Build()
    {
        IEnumerable<string> csvRows = _rows.Select(row => CreateRow(row));
        return $"{CreateHeaders()}\n{string.Join('\n', csvRows)}";
    }

    private static string CreateHeaders() => $"\"{nameof(TrainingPeaksActivity.Title)}\",\"{nameof(TrainingPeaksActivity.WorkoutType)}\",\"{nameof(TrainingPeaksActivity.WorkoutDay)}\",\"{nameof(TrainingPeaksActivity.DistanceInMeters)}\",\"{nameof(TrainingPeaksActivity.TimeTotalInHours)}\",\"{nameof(TrainingPeaksActivity.HeartRateAverage)}\",\"{nameof(TrainingPeaksActivity.HeartRateMax)}\",\"{nameof(TrainingPeaksActivity.Rpe)}\",\"{nameof(TrainingPeaksActivity.Feeling)}\"";

    private static string CreateRow(TrainingPeaksActivity row) =>
        $"\"{row.Title}\",\"{row.WorkoutType}\",\"{row.WorkoutDay.ToString(TrainingPeaksActivity.WORKOUT_DAY_DATETIME_FORMAT)}\",\"{row.DistanceInMeters}\",\"{row.TimeTotalInHours}\",\"{row.HeartRateAverage}\",\"{row.HeartRateMax}\",\"{row.Rpe}\",\"{row.Feeling}\"";

    public TrainingPeaksCsvBuilder AddRow(TrainingPeaksActivity activity)
    {
        _rows = _rows.Append(activity);
        return this;
    }

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

    public static DateTime ParseWorkoutDay(string workoutDay)
    {
        return DateTime.ParseExact(
            workoutDay,
            TrainingPeaksActivity.WORKOUT_DAY_DATETIME_FORMAT,
            CultureInfo.InvariantCulture);
    }

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
            title: "Running",
            workoutType: "Run",
            workoutDay: workoutDay,
            distanceInMeters: distanceInMeters,
            totalTimeInHours: totalTimeInHours,
            heartRateAverage: heartRateAverage,
            heartRateMax: heartRateMax,
            rpe: rpe,
            feeling: feeling
        );
    }
}
