using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;

namespace RunSuggestion.TestHelpers.Creators;

public static class RunBaseFakes
{
    /// <summary>
    /// Test helper to create a fake run event.
    /// The pattern of providing default null values for parameters and then assigning a default value within the helper
    /// Allows default values to be provided by functions, which are not allowed in method signature. 
    /// </summary>
    /// <param name="id">The UserId to assign the RunEvent to, defaults to 0</param>
    /// <param name="dateTime">The date the event took place, defaults to Now</param>
    /// <param name="distanceMetres">the distance of the run in metres - defaults to 5km</param>
    /// <param name="effort">The effort score for the run (1-10), defaults to 5</param>
    /// <param name="duration">The duration of the event as a TimeSpan.  Defaults to 30mins</param>
    /// <returns>RunEvent</returns>
    public static RunEvent CreateRunEvent(
        int? id = null,
        DateTime? dateTime = null,
        int? distanceMetres = null,
        byte? effort = null,
        TimeSpan? duration = null) =>
        new()
        {
            RunEventId = id ?? Defaults.UserId,
            Date = dateTime ?? Defaults.DateTime,
            Distance = distanceMetres ?? Defaults.DistanceMetres,
            Effort = effort ?? Defaults.Effort,
            Duration = duration ?? Defaults.Duration
        };

    /// <summary>
    /// Test helper to create a fake run recommendation.
    /// </summary>
    /// <param name="id">the unique identifier of the recommendation</param>
    /// <param name="dateTime">The date the recommendation is for, defaults to Now</param>
    /// <param name="distanceMetres">the distance of the run in metres - defaults to 5km</param>
    /// <param name="effort">The effort score for the run (1-10), defaults to 5</param>
    /// <param name="duration">The duration of the event as a TimeSpan.  Defaults to 30mins</param>
    /// <returns>RunRecommendation</returns>
    public static RunRecommendation CreateRunRecommendation(
        int? id = null,
        DateTime? dateTime = null,
        int? distanceMetres = null,
        byte? effort = null,
        TimeSpan? duration = null) =>
        new()
        {
            RunRecommendationId = id ?? Defaults.UserId,
            Date = dateTime ?? Defaults.DateTime,
            Distance = distanceMetres ?? Defaults.DistanceMetres,
            Effort = effort ?? Defaults.Effort,
            Duration = duration ?? Defaults.Duration
        };

    /// <summary>
    /// Convenience method to create multiple default run events in a single call
    /// </summary>
    /// <param name="count">The number of RunEvents to return, defaults to 1</param>
    /// <returns>Collection of RunEvents with 'count' items</returns>
    public static IEnumerable<RunEvent> CreateDefaultRunEvents(int count = 1)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateRunEvent());
    }


    /// <summary>
    /// Creates a week of run events with the specified distance and week offset from the current date.
    /// </summary>
    /// <param name="runDistance">Distance for each run in metres</param>
    /// <param name="weekEndingDate">The final day of the week (inclusive)</param>
    /// <param name="runsPerWeek">Number of runs to create for the week</param>
    /// <exception cref="ArgumentOutOfRangeException">Throws if runsPerWeek is less than 1</exception>
    /// <returns>Collection of run events for the week</returns>
    public static IEnumerable<RunEvent> CreateWeekOfRuns(int runDistance, DateTime weekEndingDate, int runsPerWeek)
    {
        const double weekLength = 7D;
        if (runsPerWeek <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(runsPerWeek), "runsPerWeek must be at least 1.");
        }
        int runSpacing = Math.Max(1, (int)Math.Floor(weekLength / runsPerWeek));

        return Enumerable.Range(0, runsPerWeek)
            .Select(runNumber => CreateRunEvent(distanceMetres: runDistance,
                                                dateTime: weekEndingDate.AddDays(-runNumber * runSpacing)));
    }

    public static RunEvent
        CreateRunEventWithPace(int distanceKm, int paceMinsPerKm, byte effort, DateTime? date = null) =>
        CreateRunEvent(distanceMetres: 1000 * distanceKm,
                       duration: TimeSpan.FromMinutes(distanceKm * paceMinsPerKm),
                       effort: effort,
                       dateTime: date);

    public static IEnumerable<RunEvent> LowIntensityRunHistory(DateTime todayDate, int sampleSize = 35) =>
        Enumerable.Range(1, sampleSize)
            .Select(index => index * -2)
            .SelectMany<int, RunEvent>(negEvens =>
            [
                CreateRunEvent(
                    dateTime: todayDate.AddDays(negEvens + 1),
                    distanceMetres: 5000,
                    effort: Runs.EffortLevel.Easy),
                CreateRunEvent(
                    dateTime: todayDate.AddDays(negEvens),
                    distanceMetres: 10000,
                    effort: Runs.EffortLevel.Recovery)
            ]);

    public static class Defaults
    {
        public const int UserId = 0;
        public const int DistanceMetres = 5000;
        public const byte Effort = 5;
        public static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);
        public static DateTime DateTime => DateTime.Now;
    }
}
