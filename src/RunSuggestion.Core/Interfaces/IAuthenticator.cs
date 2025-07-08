namespace RunSuggestion.Core.Interfaces;

public interface IAuthenticator
{
    /// <summary>
    /// Authenticates a user and returns a userId if authenticated.
    /// Returns null if user is not authenticated.
    /// </summary>
    /// <param name="token">bearer token provided by the user to authenticate</param>
    /// <returns>UserId if authentication succeeds, null if it fails.</returns>
    int? Authenticate(string token);
}
