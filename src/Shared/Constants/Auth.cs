namespace RunSuggestion.Shared.Constants;

public static class Auth
{
    public const string Header = "Authorization";
    public const string BearerTokenPrefix = "Bearer";

    public const int AllowedClockSkewMinutes = 5;
    public static readonly IEnumerable<string> AllowedProviders = [Providers.GitHub];

    public static class Providers
    {
        public const string GitHub = "github";
    }
}
