using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Core.Validators;

public class RunEventValidator
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
    public string[] Validate(IEnumerable<RunEvent> runEvents)
    {
        ArgumentNullException.ThrowIfNull(runEvents, nameof(runEvents));

        return [];
    }
}