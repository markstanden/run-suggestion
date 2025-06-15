using System.Globalization;
using CsvHelper;
using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Core.Services;

public class CsvParser : ICsvParser
{
    /// <summary>
    /// Generic method to parse the provided CSV into an IEnumerable of T.
    /// </summary>
    /// <param name="csv">The csv string to parse</param>
    /// <typeparam name="T">The model to deserialise into</typeparam>
    /// <returns>An IEnumerable of type T</returns>
    public IEnumerable<T> Parse<T>(string csv)
    {
        using StringReader reader = new(csv);
        using CsvReader csvReader = new(reader, CultureInfo.InvariantCulture);
        return csvReader.GetRecords<T>()
            .ToList();
    }
}
