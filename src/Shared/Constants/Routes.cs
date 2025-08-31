namespace RunSuggestion.Shared.Constants;

public static class Routes
{
    public const string Home = "";

    private const string AuthenticationRoot = "authentication";
    public const string Login = $"{AuthenticationRoot}/login";
    public const string Logout = $"{AuthenticationRoot}/logout";

    public const string ApiBasePath = "api/";
    public const string UploadPath = "history";
    public const string HealthCheckPath = "healthcheck";
    public const string RecommendationPath = "recommendations";
}
