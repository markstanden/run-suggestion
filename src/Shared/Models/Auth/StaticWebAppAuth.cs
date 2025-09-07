using System.Text.Json.Serialization;

namespace RunSuggestion.Shared.Models.Auth;

/// <summary>
/// The response returned by the Azure SWA /.auth/me endpoint.
/// </summary>
public record StaticWebAppsAuthDto
{
    [JsonPropertyName("clientPrincipal")] public SwaClientPrincipal? ClientPrincipal { get; init; }
}

/// <summary>
/// The ClientPrincipal object returned within the Azure SWA response,
/// minimised to what the app actually requires
/// </summary>
/// <example>
/// {
///   "identityProvider": "github",
///   "userId": "abcd12345abcd012345abcdef0123450",
///   "userDetails": "username",
///   "userRoles": ["anonymous", "authenticated"],
///   "claims": [{
///     "typ": "name",
///     "val": "Azure Static Web Apps"
///   }]
/// }
/// </example>
public record SwaClientPrincipal
{
    [JsonPropertyName("identityProvider")] public string? IdentityProvider { get; init; }
    [JsonPropertyName("userId")] public string? UserId { get; init; }
    [JsonPropertyName("userDetails")] public string? UserDetails { get; init; }
}
