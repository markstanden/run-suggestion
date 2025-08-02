using RunSuggestion.Core.Models.DataSources.TrainingPeaks;

namespace RunSuggestion.TestHelpers.Creators;

public static class TrainingPeaksActivityFakes
{
    public const int DefaultDateSpread = 2;

    /// <summary>
    /// Creates a fake TrainingPeaksActivity with sensible defaults and some randomization
    /// </summary>
    /// <returns>A TrainingPeaksActivity with realistic test data</returns>
    public static TrainingPeaksActivity CreateRandomRun(int offset = 0, int dateSpread = DefaultDateSpread)
    {
        Random random = Random.Shared;
        DateTime baseDate = DateTime.Now.AddDays(offset * -1);
        int randomDays = random.Next(dateSpread);

        return new TrainingPeaksActivity
        {
            Title = TrainingPeaksCsvBuilder.RunningTitle,
            WorkoutType = TrainingPeaksCsvBuilder.RunningWorkoutType,
            WorkoutDay = baseDate.AddDays(randomDays * -1),
            DistanceInMeters = random.Next(1000, 40000),
            TimeTotalInHours = random.NextDouble() * 1.5 + 0.5,
            HeartRateAverage = random.Next(120, 160),
            HeartRateMax = random.Next(160, 200),
            Rpe = random.Next(1, 11),
            Feeling = random.Next(1, 6)
        };
    }

    /// <summary>
    /// Creates a collection of fake TrainingPeaksActivities
    /// </summary>
    /// <param name="count">Number of activities to create</param>
    /// <param name="dateOffset">The offset for all dates in the collection - negative offset will produce dates in the future.</param>
    /// <returns>Collection of fake TrainingPeaksActivities</returns>
    public static IEnumerable<TrainingPeaksActivity> CreateRandomRuns(int count, int dateOffset = 0)
    {
        return Enumerable.Range(0, count).Select(index => CreateRandomRun(index + dateOffset));
    }
}
