using RunSuggestion.Shared.Constants;

namespace RunSuggestion.TestHelpers.Creators;

public static class AuthFakes
{
    public static string CreateAuthHeader(string? token = null)
    {
        token ??= Guid.NewGuid().ToString();
        return string.IsNullOrEmpty(Auth.BearerTokenPrefix)
            ? token
            : $"{Auth.BearerTokenPrefix} {token}";
    }
}
