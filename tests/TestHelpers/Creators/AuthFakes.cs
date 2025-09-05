using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Auth;

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

    /// <summary>
    /// Creates a fake client principal for use in unit tests.
    /// Provides defaults values for all parameters, but with overridable values for testing.
    /// </summary>
    /// <param name="userId">Sets the userId in the Client principal, defaults to Any.LongAlphaNumericString</param>
    /// <param name="identityProvider">Sets the identityProvider in the Client principal, defaults to Any.ShortAlphaNumericString</param>
    /// <param name="userDetails">Sets the userDetails in the Client principal, defaults to Any.ShortAlphaWithSpecialCharsString</param>
    /// <returns><see cref="SwaClientPrincipal"/>With all values set</returns>
    public static SwaClientPrincipal CreateFakeClientPrincipal(
        string? userId = null,
        string? identityProvider = null,
        string? userDetails = null) =>
        new()
        {
            UserId = userId ?? Any.LongAlphanumericString,
            IdentityProvider = identityProvider ?? Auth.ValidIssuers.First(),
            UserDetails = userDetails ?? Any.ShortAlphaWithSpecialCharsString
        };


    public static string CreateFakeBase64SwaClientPrincipal(
        string? userId = null,
        string? identityProvider = null,
        string? userDetails = null)
    {
        SwaClientPrincipal principal = CreateFakeClientPrincipal(userId, identityProvider, userDetails);
        return EncodePrincipalToTokenString(principal);
    }

    public static string EncodePrincipalToTokenString(SwaClientPrincipal principal)
    {
        string json = JsonSerializer.Serialize(principal);
        return Base64UrlEncoder.Encode(json);
    }
}
