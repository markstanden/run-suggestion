using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Core.Validators;

public class RunEventValidator: IValidator<RunEvent>
{
    private DateTime _currentDate;
    
    public RunEventValidator(DateTime? currentDate)
    {
        _currentDate = currentDate ?? DateTime.Now;
    }

    /// <summary>
    /// Validates a collection of run events
    /// </summary>
    /// <param name="runEvents">Collection of RunEvents to validate</param>
    /// <returns>true if all valid</returns>
    /// <exception cref="ArgumentException">Throws an argument exception detailing which RunEvent property is invalid</exception>
    public IEnumerable<string> Validate(IEnumerable<RunEvent> runEvents)
    {
        ArgumentNullException.ThrowIfNull(runEvents, nameof(runEvents));

        return runEvents.SelectMany((runEvent, index) => { 
            List<string> validationErrors = new();
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
    
    private bool IsValidDate(DateTime date) => date <= _currentDate;
    private bool IsValidDistance(int distance) => distance > 0;
    private bool IsValidEffort(byte effort) => effort <= 10;
    private bool IsValidDuration(TimeSpan duration) => duration > TimeSpan.Zero;
}