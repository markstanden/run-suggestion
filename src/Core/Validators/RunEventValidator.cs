using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Models.Runs;

namespace RunSuggestion.Core.Validators;

public class RunEventValidator : IValidator<RunEvent>
{
    private readonly DateTime _currentDate;

    public RunEventValidator(DateTime? currentDate)
    {
        _currentDate = currentDate ?? DateTime.Now;
    }

    /// <summary>
    /// Validates a collection of run events.
    /// Iterates the entire collection and checks each row against required data rules.
    /// Errors are returned as a single (flat) collection of errors, each error prefixed with the index of the failing row
    /// </summary>
    /// <param name="runEvents">Collection of RunEvents to validate</param>
    /// <returns>Collection of validation error messages, empty if all valid.</returns>
    /// <exception cref="ArgumentNullException">Throws an argument null exception if the provided collection is null</exception>
    public IEnumerable<string> Validate(IEnumerable<RunEvent?> runEvents)
    {
        ArgumentNullException.ThrowIfNull(runEvents);

        return runEvents.SelectMany((runEvent, index) =>
        {
            List<string> validationErrors = [];

            if (runEvent is null)
            {
                validationErrors.Add($"{index}: Invalid run event - cannot be null");
                return validationErrors;
            }

            if (!IsValidDate(runEvent.Date))
            {
                validationErrors.Add($"{index}: Invalid run date {runEvent.Date:d} - cannot be in the future");
            }

            if (!IsValidDistance(runEvent.Distance))
            {
                validationErrors.Add($"{index}: Invalid run distance - it must be a positive integer");
            }

            if (!IsValidEffort(runEvent.Effort))
            {
                validationErrors.Add($"{index}: Invalid run effort - it must be between 0 and 10");
            }

            if (!IsValidDuration(runEvent.Duration))
            {
                validationErrors.Add($"{index}: Invalid run duration - it must be greater than 0");
            }
            return validationErrors;
        });
    }

    /// <summary>
    /// A valid RunEvent date is either today or in the past.
    /// Uses the instance _currentDate to allow for injected dates for
    /// consistent testing results.
    /// </summary>
    /// <param name="date">The date to validate</param>
    /// <returns>true if valid</returns>
    private bool IsValidDate(DateTime date) => date <= _currentDate;

    /// <summary>
    /// A valid RunEvent distance is greater than 0 metres, with no ceiling.
    /// </summary>
    /// <param name="distance">The distance to validate</param>
    /// <returns>true if valid</returns>
    private static bool IsValidDistance(int distance) => distance > 0;

    /// <summary>
    /// A valid RunEvent effort is 0 or greater, but with a max value of 10.
    /// </summary>
    /// <param name="effort">The effort score to validate</param>
    /// <returns>true if valid</returns>
    private static bool IsValidEffort(byte effort) => effort <= 10;

    /// <summary>
    /// A valid RunEvent duration is greater than 0, with currently no minimum or maximum length.
    /// </summary>
    /// <param name="duration">The duration to validate</param>
    /// <returns>true if valid</returns>
    private static bool IsValidDuration(TimeSpan duration) => duration > TimeSpan.Zero;
}
