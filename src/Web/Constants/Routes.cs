namespace RunSuggestion.Web.Constants;

public static class Routes
{
    internal const string Home = "";

    private const string AuthenticationRoot = "authentication";
    internal const string Login = $"{AuthenticationRoot}/login";
    internal const string Logout = $"{AuthenticationRoot}/logout";

    private const string ApiRoot = "api";
    internal const string UploadApiEndpoint = $"{ApiRoot}/history";
    internal const string RecommendationApiEndpoint = $"{ApiRoot}/recommendation";
}
