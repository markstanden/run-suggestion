namespace RunSuggestion.Api.Extensions;

public static class StringExtensions
{
    private const int SubstringLength = 5;

    /// <summary>
    /// Returns the last 5 characters of the passed string
    /// </summary>
    /// <param name="fullString">the full string to extract from</param>
    /// <returns>the last 5 characters of the passed string</returns>
    public static string LastFiveChars(this string fullString) =>
        fullString.Length <= SubstringLength
            ? fullString
            : fullString[^SubstringLength..];
}
