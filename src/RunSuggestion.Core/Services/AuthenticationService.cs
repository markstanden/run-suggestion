using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Core.Services;

public class AuthenticationService : IAuthenticator
{
    /// <inheritdoc/>
    /// Currently a stub implementation for development, so always returns the first 16 characters of the provided token.
    public string? Authenticate(string? token)
    {
        // TODO: Validate token

        // TODO: Extract entraId from token
        
        // TODO: Return real token - for now we'll just return the first 16 characters of the passed token
        return token?.Substring(0, 16);
    }
}
