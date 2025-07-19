namespace RunSuggestion.Core.Unit.Tests.TestHelpers.Doubles;

public static class RunEventFakes
{
    /// <summary>
    /// Test helper to create a fake run event.
    /// The pattern of providing default null values for parameters and then assigning a default value within the helper
    /// Allows default values to be provided by functions, which are not allowed in method signature. 
    /// </summary>
    /// <param name="userId">The UserId to assign the RunEvent to, defaults to 0</param>
    /// <param name="dateTime">The date the event took place, defaults to Now</param>
    /// <param name="distanceMetres">the distance of the run in metres - defaults to 5km</param>
    /// <param name="effort">The effort score for the run (1-10), defaults to 5</param>
    /// <param name="duration">The duration of the event as a TimeSpan.  Defaults to 30mins</param>
    /// <returns>RunEvent</returns>
    public static Models.Runs.RunEvent CreateRunEvent(
        int? userId = null,
        DateTime? dateTime = null,
        int? distanceMetres = null,
        byte? effort = null,
        TimeSpan? duration = null) => new()
        {
            RunEventId = userId ?? 0,
            Date = dateTime ?? DateTime.Now,
            Distance = distanceMetres ?? 5000,
            Effort = effort ?? 5,
            Duration = duration ?? TimeSpan.FromMinutes(30)
        };

    /// <summary>
    /// Convenience method to create multiple default run events in a single call
    /// </summary>
    /// <param name="count">The number of RunEvents to return, defaults to 1</param>
    /// <returns>Collection of RunEvents with 'count' items</returns>
    public static IEnumerable<Models.Runs.RunEvent> CreateRunEvents(int count = 1)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateRunEvent());
    }
}
