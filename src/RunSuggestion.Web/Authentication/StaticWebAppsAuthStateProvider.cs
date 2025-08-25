using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using RunSuggestion.Web.Authentication.Models;

namespace RunSuggestion.Web.Authentication;

public class StaticWebAppsAuthStateProvider(
    HttpClient httpClient,
    ILogger<StaticWebAppsAuthStateProvider> logger
) : AuthenticationStateProvider
{
    internal const string UnknownAuthProvider = "UNKNOWN";
    internal const string DefaultUserName = "Anonymous User";
    internal const string AuthTypeSwa = "swa";
    internal const string ClaimTypeIdentityProvider = "provider";
    internal const string RequestUri = "/.auth/me";

    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static AuthenticationState FailAuthentication() => new(new ClaimsPrincipal(new ClaimsIdentity()));

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Make the authentication request and progress as soon as the headers are received.
            // The .auth/me/ endpoint will approve the token and return the contents as json.
            using HttpResponseMessage response = await _httpClient.GetAsync(
                RequestUri,
                HttpCompletionOption.ResponseHeadersRead
            );

            // Check that the authentication request was successful
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Authentication request failed with status code: {StatusCode}",
                    response.StatusCode
                );
                return FailAuthentication();
            }

            // Response was a success code, so read the body content.
            await using Stream stream = await response.Content.ReadAsStreamAsync();

            // Deserialize the JSON response into our expected token object.
            StaticWebAppsAuthDto? authData = await JsonSerializer.DeserializeAsync<StaticWebAppsAuthDto>(
                stream,
                JsonSerializerOptions
            );

            // If the authentication token doesn't include a `UserId` we should fail fast
            // as this is used to identify the user.
            if (string.IsNullOrWhiteSpace(authData?.ClientPrincipal?.UserId))
            {
                _logger.LogError("Authentication request failed. UserId is missing from token.");
                return FailAuthentication();
            }

            string userName = string.IsNullOrWhiteSpace(authData.ClientPrincipal.UserDetails)
                ? DefaultUserName
                : authData.ClientPrincipal.UserDetails;

            string userAuthProvider = string.IsNullOrWhiteSpace(authData.ClientPrincipal.IdentityProvider)
                ? UnknownAuthProvider
                : authData.ClientPrincipal.IdentityProvider;

            // Create a new ClaimsIdentity and ClaimsPrincipal from the contents of the token.
            // I have minimised claims here to increase user anonymity
            List<Claim> claims =
            [
                // We need to check this is present as this is part of the unique user identification
                new(ClaimTypes.NameIdentifier, authData.ClientPrincipal.UserId),

                // Used in the UI so the user can identify themselves (know who they are logged in as)
                new(ClaimTypes.Name, userName),

                // This is also required for user identification, to prevent potential UserId collisions between providers
                new(ClaimTypeIdentityProvider, userAuthProvider)
            ];

            ClaimsIdentity identity = new(claims, AuthTypeSwa);
            ClaimsPrincipal user = new(identity);

            _logger.LogInformation("Authentication request successful");
            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            // Catch any exceptions during authentication and return a failed authentication state.
            _logger.LogError(ex, "Failed to get authentication state");
            return FailAuthentication();
        }
    }
}
