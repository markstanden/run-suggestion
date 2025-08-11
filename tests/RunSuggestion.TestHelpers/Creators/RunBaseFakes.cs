using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.TestHelpers.Creators;

public static class RunBaseFakes
{
    public static class Defaults
    {
        public const int UserId = 0;
        public const int DistanceMetres = 5000;
        public const byte Effort = 5;
        public static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);
        public static DateTime DateTime => DateTime.Now;
    }

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
        TimeSpan? duration = null) => new()
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
        TimeSpan? duration = null) => new()
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
    public static IEnumerable<RunEvent> CreateRunEvents(int count = 1)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateRunEvent());
    }
}
