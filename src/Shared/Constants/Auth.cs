namespace RunSuggestion.Shared.Constants;

public static class Auth
{
    public static class JwtConfig
    {
        public const string JwtHeader = "Authorization";
        public const string BearerTokenPrefix = "Bearer";
        public const int AllowedClockSkewMinutes = 5;
    }

    public static class SwaConfig
    {
        public const string SwaHeader = "x-ms-client-principal";
    }

    public const string Header = SwaConfig.SwaHeader;
    public static readonly IEnumerable<string> ValidIssuers = [Issuers.GitHub];

    public static class Issuers
    {
        public const string GitHub = "github";
    }
}
