namespace RunSuggestion.Web.Authentication;

public static class StaticWebApp
{
    public const string SwaRootAuthPath = ".auth";
    public const string DefaultProvider = "github";

    public const string LoginBasePath = $"{SwaRootAuthPath}/login/{DefaultProvider}";
    public const string LoginRedirectQuery = "post_login_redirect_url";

    public const string LogoutBasePath = $"{SwaRootAuthPath}/logout/";
    public const string LogoutRedirectQuery = "post_logout_redirect_url";


    /// <summary>
    /// Builds the StaticWebApp login path including the redirect query using the specified redirect URL.
    /// If passed redirect is null or empty string the query is omitted
    /// </summary>
    /// <param name="redirect">The URL to be redirected to following a successful login.</param>
    /// <returns>Returns the full login path including the redirect query parameter if provided.</returns>
    public static string LoginPath(string? redirect) =>
        PathWithRedirectQuery(LoginBasePath, LoginRedirectQuery, redirect);

    /// <summary>
    /// Builds the StaticWebApp logout path including the redirect query using the specified redirect URL.
    ///     /// If passed redirect is null or empty string the query is omitted
    /// </summary>
    /// <param name="redirect">The URL to be redirected to following a successful logout.</param>
    /// <returns>Returns the full logout path including the redirect query parameter if provided.</returns>
    public static string LogoutPath(string? redirect) =>
        PathWithRedirectQuery(LogoutBasePath, LogoutRedirectQuery, redirect);

    private static string PathWithRedirectQuery(string basePath, string query, string? redirect)
        => string.IsNullOrWhiteSpace(redirect)
            ? basePath
            : $"{basePath}/{query}={Uri.EscapeDataString(redirect)}";
}
