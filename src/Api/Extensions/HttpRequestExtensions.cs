using Microsoft.AspNetCore.Http;
using RunSuggestion.Shared.Constants;

namespace RunSuggestion.Api.Extensions;

public static class HttpRequestExtensions
{
    /// <summary>
    /// Extracts the value of the Authorization header from the provided HttpRequest object.
    /// </summary>
    /// <param name="request">The HttpRequest containing the Authorization header.</param>
    /// <returns>The Authorization header as a string, returns an empty string if null</returns>
    public static string GetAuthHeader(this HttpRequest? request) =>
        request?.Headers[Auth.Header].ToString() ?? string.Empty;
}
