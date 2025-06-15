using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Core.Services;

public class AuthenticationService : IAuthenticator
{
    public int? Authenticate(string token)
    {
        // Validate any token for now, return userId 1
        return 1;

        // TODO: Validate token

        // TODO: Lookup userId

        // TODO: return userId
    }
}
