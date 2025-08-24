namespace RunSuggestion.Web.Constants;

public static class StaticWebApp
{
    public static class Auth
    {
        public const string AuthPath = ".auth";
        public const string DefaultProvider = "aad";
        public const string Login = $"{AuthPath}/login/{DefaultProvider}";
        public const string Logout = $"{AuthPath}/logout/";
    }
}
