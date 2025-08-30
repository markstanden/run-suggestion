using RunSuggestion.Shared.Models.Runs;

namespace RunSuggestion.Core.Interfaces;

public interface IRunHistoryTransformer
{
    /// <summary>
    /// Transforms a csv string into an IEnumerable of RunEvents.
    /// </summary>
    /// <param name="csv">The CSV string to be parsed</param>
    /// <returns>an IEnumerable of RunEvents</returns>
    IEnumerable<RunEvent> Transform(string csv);
}
