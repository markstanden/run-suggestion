using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.TestHelpers.Assertions;

public static class RunEventAssertions
{
    /// <summary>
    /// Custom assertion to compare RunEvent collections.
    /// Persisted RunEvents have an Id set by the database, which is present in the collection returned by the repository.
    /// These RunEventIds must be ignored in testing as they are not known prior to being created.
    /// </summary>
    /// <param name="actual">The collection of run events to compare</param>
    /// <param name="expected">The collection to compare against</param>
    public static void ShouldMatchRunEvents(this IEnumerable<RunEvent> actual, IEnumerable<RunEvent> expected)
    {
        // By normalising the RunEvents first, Shouldly's native assertion, including the ignoreOrder flag, can be harnessed.
        IEnumerable<RunEvent> normalizedActual = actual.Select(r => r with { RunEventId = 0 });
        IEnumerable<RunEvent> normalizedExpected = expected.Select(r => r with { RunEventId = 0 });

        normalizedActual.ShouldBe(normalizedExpected, true);
    }
}
