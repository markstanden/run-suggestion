using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using RunSuggestion.Shared.Constants;

namespace RunSuggestion.TestHelpers.Creators;

public static class AuthFakes
{
    public static string CreateAuthHeader(string? token = null)
    {
        token ??= CreateToken();
        return string.Equals(Auth.Header, Auth.SwaConfig.SwaHeader) ||
               string.IsNullOrEmpty(Auth.JwtConfig.BearerTokenPrefix)
            ? token
            : $"{Auth.JwtConfig.BearerTokenPrefix} {token}";
    }

    public static string CreateToken(string? userId = null, string? identityProvider = null, string? userDetails = null,
        string[]? userRoles = null)
    {
        string token = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["userId"] = userId ?? Guid.NewGuid().ToString(),
            ["identityProvider"] = identityProvider ?? Any.FiveCharString,
            ["userDetails"] = userDetails ?? Any.ShortAlphanumericString,
            ["userRoles"] = userRoles ?? ["anonymous", "authenticated"]
        });
        return Base64UrlEncoder.Encode(token);
    }
}
