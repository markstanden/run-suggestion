using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models;

namespace RunSuggestion.Core.Transformers;

public class CsvToRunHistoryTransformer : IRunHistoryTransformer
{
    public IEnumerable<RunEvent> Transform(string csv)
    {
        throw new NotImplementedException();
    }
}
