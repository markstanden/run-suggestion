namespace RunSuggestion.Web.Authentication.Models;

/// <summary>
/// The response returned by the Azure SWA /.auth/me endpoint.
/// </summary>
public class StaticWebAppsAuth
{
    public SwaClientPrincipal? ClientPrincipal { get; init; }
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
public class SwaClientPrincipal
{
    public string? IdentityProvider { get; init; }
    public string? UserId { get; init; }
    public string? UserDetails { get; init; }
}
