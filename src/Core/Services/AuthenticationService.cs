using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Constants;

namespace RunSuggestion.Core.Services;

public class AuthenticationService() : IAuthenticator
{
    /// <inheritdoc/>
    /// Currently a stub implementation for development, so always returns the first 16 characters of the provided token.
    public string? Authenticate(string? token)
    {
        try
        {
            ExtractToken(token);
        }
        catch
        {
            return null;
        }
        // TODO: Extract entraId from token

        // TODO: Return real token - for now we'll just return the first 16 characters of the passed token

        return token?.Substring(0, 16);
    }

    internal static string ExtractToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException(Errors.Authentication.NullOrWhitespaceToken, nameof(token));
        }

        string trimmedToken = token.Trim();

        if (!trimmedToken.StartsWith(Auth.BearerTokenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(Errors.Authentication.InvalidToken, nameof(token));
        }

        return trimmedToken[Auth.BearerTokenPrefix.Length..]
            .TrimStart();
    }
}
