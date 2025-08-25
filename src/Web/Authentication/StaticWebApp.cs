namespace RunSuggestion.Web.Authentication;

public static class StaticWebApp
{
    public const string SwaRootAuthPath = "/.auth";
    public const string DefaultProvider = "github";

    public const string LoginBasePath = $"{SwaRootAuthPath}/login/{DefaultProvider}";
    public const string LoginRedirectQuery = "post_login_redirect_url";

    public const string LogoutBasePath = $"{SwaRootAuthPath}/logout";
    public const string LogoutRedirectQuery = "post_logout_redirect_url";


    /// <summary>
    /// Builds the StaticWebApp login path including the redirect query using the specified redirect URL.
    /// If passed redirect is null or empty string the query is omitted
    /// </summary>
    /// <param name="redirect">The URL to be redirected to following a successful login.</param>
    /// <returns>Returns the full login path including the redirect query parameter if provided.</returns>
    public static string LoginPath(string? redirect) =>
        PathWithQuery(LoginBasePath, LoginRedirectQuery, redirect);

    /// <summary>
    /// Builds the StaticWebApp logout path including the redirect query using the specified redirect URL.
    ///     /// If passed redirect is null or empty string the query is omitted
    /// </summary>
    /// <param name="redirect">The URL to be redirected to following a successful logout.</param>
    /// <returns>Returns the full logout path including the redirect query parameter if provided.</returns>
    public static string LogoutPath(string? redirect) =>
        PathWithQuery(LogoutBasePath, LogoutRedirectQuery, redirect);

    /// <summary>
    /// Builds the StaticWebApp authentication path with the redirect query parameter if provided.
    /// </summary>
    /// <param name="basePath">The base path of the auth action</param>
    /// <param name="query">The query parameter to be added to the path</param>
    /// <param name="value">The value of the provided query</param>
    /// <returns>Returns the full path including the query parameter if provided.</returns>
    private static string PathWithQuery(string basePath, string query, string? value)
        => string.IsNullOrWhiteSpace(value)
            ? basePath
            : $"{basePath}?{query}={Uri.EscapeDataString(value.Trim())}";
}
