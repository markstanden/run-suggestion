using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using RunSuggestion.Web.Authentication.Models;

namespace RunSuggestion.Web.Authentication;

public class StaticWebAppsAuthStateProvider(HttpClient httpClient, ILogger<StaticWebAppsAuthStateProvider> logger)
    : AuthenticationStateProvider
{
    private const string UnknownAuthProvider = "UNKNOWN";
    private const string DefaultUserName = "Anonymous User";
    private const string AuthTypeSwa = "swa";
    private const string ClaimTypeProvider = "provider";
    private const string RequestUri = "/.auth/me";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    private static AuthenticationState FailAuthentication() => new(new ClaimsPrincipal());

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Make the authentication request and progress as soon as the headers are received.
            // The .auth/me/ endpoint will approve the token and return the contents as json.
            using HttpResponseMessage response =
                await httpClient.GetAsync(RequestUri, HttpCompletionOption.ResponseHeadersRead);

            // Check that the authentication request was successful
            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Authentication request failed with status code: {StatusCode}", response.StatusCode);
                return FailAuthentication();
            }

            // Response was a success code, so read the body content.
            await using Stream stream = await response.Content.ReadAsStreamAsync();

            // Deserialize the JSON response into our expected token object.
            StaticWebAppsAuth? authData =
                await JsonSerializer.DeserializeAsync<StaticWebAppsAuth>(stream, JsonSerializerOptions);

            // If the authentication token doesn't include a `UserId` we should fail fast
            // as this is used to identify the user. 
            if (authData?.ClientPrincipal?.UserId is null)
            {
                logger.LogError("Authentication request failed. UserId is missing from token.");
                return FailAuthentication();
            }


            // Create a new ClaimsIdentity and ClaimsPrincipal from the contents of the token.
            // I have minimised claims here to increase user anonymity
            List<Claim> claims =
            [
                // Used in the UI so the user can identify themselves (know who they are logged in as)
                new(ClaimTypes.Name, authData.ClientPrincipal.UserDetails ?? DefaultUserName),

                // We need to check this is present as this is part of the unique user identification
                new(ClaimTypes.NameIdentifier, authData.ClientPrincipal.UserId),

                // This is also required for user identification, to prevent potential UserId collisions between providers
                new(ClaimTypeProvider, authData.ClientPrincipal.IdentityProvider ?? UnknownAuthProvider)
            ];

            ClaimsIdentity identity = new(claims, AuthTypeSwa);
            ClaimsPrincipal user = new(identity);

            logger.LogInformation("Authentication request successful");
            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            // Catch any exceptions during authentication and return a failed authentication state.
            logger.LogError(ex, "Failed to get authentication state");
            return FailAuthentication();
        }
    }
}
