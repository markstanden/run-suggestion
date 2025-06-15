namespace RunSuggestion.Core.Interfaces;

public interface ICsvParser
{
    /// <summary>
    /// Generic method to parse the provided CSV into an IEnumerable of T.
    /// </summary>
    /// <param name="csv">The csv string to parse</param>
    /// <typeparam name="T">The model to deserialise into</typeparam>
    /// <returns>An IEnumerable of type T</returns>
    IEnumerable<T> Parse<T>(string csv);
}
