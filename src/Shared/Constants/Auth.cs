namespace RunSuggestion.Shared.Constants;

public static class Auth
{
    public const string Header = "Authorization";
    public const string BearerTokenPrefix = "Bearer";

    public const int AllowedClockSkewMinutes = 5;
    public static readonly IEnumerable<string> ValidIssuers = [Issuers.GitHub];

    public static class Issuers
    {
        public const string GitHub = "github";
    }
}
