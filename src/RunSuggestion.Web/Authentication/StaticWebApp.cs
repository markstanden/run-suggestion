namespace RunSuggestion.Web.Authentication;

public static class StaticWebApp
{
    private const string AuthPath = ".auth";
    private const string DefaultProvider = "github";

    private const string LoginBasePath = $"{AuthPath}/login/{DefaultProvider}";
    private const string LoginRedirectQuery = "post_login_redirect_url";

    private const string LogoutBasePath = $"{AuthPath}/logout/";
    private const string LogoutRedirectQuery = "post_logout_redirect_url";


    public static string LoginPath(string redirect) => $"{LoginBasePath}/{LoginRedirectQuery}={redirect}";
    public static string LogoutPath(string redirect) => $"{LogoutBasePath}/{LogoutRedirectQuery}={redirect}";
}
