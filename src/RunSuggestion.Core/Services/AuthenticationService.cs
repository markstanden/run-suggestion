using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Core.Services;

public class AuthenticationService : IAuthenticator
{
    /// <summary>
    /// Authenticates a user and returns a userId if authenticated.
    /// Returns null if user is not authenticated.
    /// Currently a stub implementation for development, so always returns 1.
    /// </summary>
    /// <param name="token">bearer token provided by the user to authenticate</param>
    /// <returns>UserId if authentication succeeds, null if it fails. Currently prototype always returns 1</returns>
    public int? Authenticate(string token)
    {
        // Validate any token for now, return userId 1
        return 1;

        // TODO: Validate token

        // TODO: Lookup userId

        // TODO: return userId
    }
}
