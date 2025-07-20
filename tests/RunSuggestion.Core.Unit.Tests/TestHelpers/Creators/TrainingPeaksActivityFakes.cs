using RunSuggestion.Core.Models.DataSources.TrainingPeaks;

namespace RunSuggestion.Core.Unit.Tests.TestHelpers.Doubles;

public static class TrainingPeaksActivityFakes
{
    /// <summary>
    /// Creates a fake TrainingPeaksActivity with sensible defaults and some randomization
    /// </summary>
    /// <returns>A TrainingPeaksActivity with realistic test data</returns>
    public static TrainingPeaksActivity CreateRandomRun()
    {
        Random random = Random.Shared;
        DateTime baseDate = new(2025, 1, 1);
        int randomDays = random.Next(365);

        return new TrainingPeaksActivity
        {
            Title = TrainingPeaksCsvBuilder.RunningTitle,
            WorkoutType = TrainingPeaksCsvBuilder.RunningWorkoutType,
            WorkoutDay = baseDate.AddDays(randomDays),
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
    /// <returns>Collection of fake TrainingPeaksActivities</returns>
    public static IEnumerable<TrainingPeaksActivity> CreateRandomRuns(int count)
    {
        return Enumerable.Range(0, count).Select(_ => CreateRandomRun());
    }
}
