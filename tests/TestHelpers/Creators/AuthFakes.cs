using RunSuggestion.Shared.Constants;

namespace RunSuggestion.TestHelpers.Creators;

public static class AuthFakes
{
    public static string CreateJwtAuthHeader(string? token = null)
    {
        token ??= Guid.NewGuid().ToString();
        return string.IsNullOrEmpty(Auth.JwtConfig.BearerTokenPrefix)
            ? token
            : $"{Auth.JwtConfig.BearerTokenPrefix} {token}";
    }
}
