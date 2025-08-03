using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Core.Services;

public class CsvParser : ICsvParser
{
    private readonly CsvConfiguration _config = new(CultureInfo.InvariantCulture)
    {
        HeaderValidated = null,
        MissingFieldFound = null
    };

    /// <summary>
    /// Generic method to parse the provided CSV into an IEnumerable of T.
    /// </summary>
    /// <param name="csv">The csv string to parse</param>
    /// <typeparam name="T">The model to deserialise into</typeparam>
    /// <returns>An IEnumerable of type T</returns>
    public IEnumerable<T> Parse<T>(string csv)
    {
        using StringReader reader = new(csv);
        using CsvReader csvReader = new(reader, _config);

        csvReader.Context.TypeConverterOptionsCache
            .GetOptions<double?>()
            .NullValues.Add("");

        csvReader.Context.TypeConverterOptionsCache
            .GetOptions<int?>()
            .NullValues.Add("");

        return csvReader.GetRecords<T>()
            .ToList();
    }
}
