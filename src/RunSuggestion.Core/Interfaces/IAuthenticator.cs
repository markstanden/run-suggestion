namespace RunSuggestion.Core.Interfaces;

public interface IAuthenticator
{
    /// <summary>
    /// Authenticates a user and returns the user's entraId if authenticated.
    /// Returns null if user is not authenticated.
    /// </summary>
    /// <param name="token">bearer token provided by the user to authenticate</param>
    /// <returns>EntraId if authentication succeeds, null if it fails.</returns>
    string? Authenticate(string token);
}
