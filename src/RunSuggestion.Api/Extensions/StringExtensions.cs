namespace RunSuggestion.Api.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Returns the last 5 characters of the passed string
    /// </summary>
    /// <param name="fullString">the full string to truncate</param>
    /// <returns>the last 5 characters of the passed string</returns>
    public static string LastFiveChars(this string fullString) => fullString[^5..];
}
