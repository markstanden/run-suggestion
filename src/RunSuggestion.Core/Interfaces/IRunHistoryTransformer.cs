using RunSuggestion.Core.Models;

namespace RunSuggestion.Core.Interfaces;

public interface IRunHistoryTransformer
{
    IEnumerable<RunEvent> Transform(string csv);
}
