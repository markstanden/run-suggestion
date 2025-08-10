using Microsoft.AspNetCore.Http;

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

    /// <summary>
    /// Extracts the value of the Authorization header from the provided HttpRequest object.
    /// </summary>
    /// <param name="request">The HttpRequest containing the Authorization header.</param>
    /// <returns>The Authorization header as a string, returns an empty string if null</returns>
    public static string GetAuthHeaderFromRequest(HttpRequest? request) =>
        request?.Headers.Authorization.ToString() ?? string.Empty;
}
