using System.Diagnostics.Contracts;
using Microsoft.IdentityModel.Tokens;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Auth;
using System.Text.Json;

namespace RunSuggestion.Core.Services;

public class SwaAuthenticationService : IAuthenticator
{
    /// <inheritdoc/>
    public string? Authenticate(string? token)
    {
        try
        {
            string json = ExtractJson(token);
            Console.WriteLine(json);
            SwaClientPrincipal principal = ValidateToken(json);
            string id = ParseIdFromPrincipal(principal);
            return id;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Cleans and extracts the actual json formatted token from the provided authentication header string.
    /// </summary>
    /// <param name="jsonToken">The base64 encoded token as found within the swa authentication header</param>
    /// <returns>a cleaned token as plaintext json ready for deserialisation</returns>
    /// <exception cref="ArgumentException">Throws ArgumentException if the passed header value is Null or Whitespace</exception>
    [Pure]
    internal static string ExtractJson(string? jsonToken)
    {
        if (string.IsNullOrWhiteSpace(jsonToken))
        {
            throw new ArgumentException(Errors.Authentication.NullOrWhitespaceToken, nameof(jsonToken));
        }

        string cleanToken = jsonToken.Trim();
        return Base64UrlEncoder.Decode(cleanToken);
    }

    /// <summary>
    /// Parses and validates the provided SWA authentication json encoded principal.
    /// Validates that the passed JSON fits the expected <see cref="SwaClientPrincipal"/> model, and that the expected claims are present.
    /// All serialisation errors are caught and rethrown as a SecurityTokenException.
    /// </summary>
    /// <param name="jsonPrincipal">The cleaned token string</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>A valid JwtSecurityToken</returns>
    /// <exception cref="SecurityTokenException">Throws a SecurityTokenException if the principal fails serialisation or is invalid</exception>
    [Pure]
    internal static SwaClientPrincipal ValidateToken(string jsonPrincipal)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonPrincipal))
            {
                throw new ArgumentException(Errors.Authentication.NullOrWhitespaceToken, nameof(jsonPrincipal));
            }

            SwaClientPrincipal? principal = JsonSerializer.Deserialize<SwaClientPrincipal>(jsonPrincipal);

            // If the userId is empty or the issuer is not in the allowed list, throw an exception.
            if (string.IsNullOrWhiteSpace(principal?.UserId) ||
                !Auth.ValidIssuers.Contains(principal.IdentityProvider))
            {
                throw new SecurityTokenException(Errors.Authentication.InvalidToken);
            }

            return principal;
        }
        catch (JsonException)
        {
            throw new SecurityTokenException(Errors.Authentication.InvalidToken);
        }
    }

    /// <summary>
    /// Creates a composite Id from a validated principal - first checking that the required claims are present.
    /// As we could accept multiple providers, we need to return a composite Id that includes
    /// both the provider and the subject to prevent potential id collisions.
    /// </summary>
    /// <param name="principal">A previously validated SWA principal</param>
    /// <returns>The custom ID based on the provided principal's `IdentityProvider` and `UserId` combination</returns>
    /// <exception cref="ArgumentException">Throws an ArgumentException if any of the required claims are null or whitespace</exception>
    [Pure]
    internal static string ParseIdFromPrincipal(SwaClientPrincipal principal)
    {
        if (string.IsNullOrWhiteSpace(principal.UserId) ||
            string.IsNullOrWhiteSpace(principal.IdentityProvider))
        {
            throw new ArgumentException(Errors.Authentication.InvalidToken, nameof(principal));
        }
        return $"{principal.IdentityProvider}:{principal.UserId}";
    }
}
