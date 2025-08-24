namespace RunSuggestion.Web.Authentication;

public static class StaticWebApp
{
    private const string AuthPath = ".auth";
    private const string DefaultProvider = "github";

    private const string LoginBasePath = $"{AuthPath}/login/{DefaultProvider}";
    private const string LoginRedirectQuery = "post_login_redirect_url";

    private const string LogoutBasePath = $"{AuthPath}/logout/";
    private const string LogoutRedirectQuery = "post_logout_redirect_url";


    /// <summary>
    /// Builds the StaticWebApp login path including the redirect query using the specified redirect URL.
    /// </summary>
    /// <param name="redirect">The URL to be redirected to following a successful login.</param>
    /// <returns>Returns the full login path including the redirect query parameter.</returns>
    public static string LoginPath(string? redirect) => $"{LoginBasePath}/{LoginRedirectQuery}={redirect}";

    /// <summary>
    /// Builds the StaticWebApp logout path including the redirect query using the specified redirect URL.
    /// </summary>
    /// <param name="redirect">The URL to be redirected to following a successful logout.</param>
    /// <returns>Returns the full logout path including the redirect query parameter.</returns>
    public static string LogoutPath(string? redirect) => $"{LogoutBasePath}/{LogoutRedirectQuery}={redirect}";
}
