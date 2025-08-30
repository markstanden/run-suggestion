namespace RunSuggestion.Shared.Constants;

public static class Routes
{
    public const string Home = "";

    private const string AuthenticationRoot = "authentication";
    public const string Login = $"{AuthenticationRoot}/login";
    public const string Logout = $"{AuthenticationRoot}/logout";

    private const string ApiRoot = "api";
    public const string UploadPath = "history";
    public const string UploadApiEndpoint = $"{ApiRoot}/{UploadPath}";
    public const string HealthCheckPath = "healthcheck";
    public const string HealthCheckApiEndpoint = $"{ApiRoot}/{HealthCheckPath}";
    public const string RecommendationPath = "recommendation";
    public const string RecommendationApiEndpoint = $"{ApiRoot}/{RecommendationPath}";
}
