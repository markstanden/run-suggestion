namespace RunSuggestion.Api.Helpers;

public static class AuthHelpers
{
    private const string EmptyEntraId = "<EMPTY>";

    /// <summary>
    /// Returns the last 5 characters of the passed string
    /// </summary>
    /// <param name="fullString"></param>
    /// <returns>the last 5 characters of the passed string</returns>
    public static string GetLastFiveChars(string? fullString) => fullString?[^5..] ?? EmptyEntraId;
}
