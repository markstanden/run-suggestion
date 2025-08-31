using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Constants;

namespace RunSuggestion.Core.Services;

public class AuthenticationService(JwtSecurityTokenHandler? tokenHandler = null) : IAuthenticator
{
    private readonly JwtSecurityTokenHandler _tokenHandler = tokenHandler ?? new JwtSecurityTokenHandler();

    private readonly TokenValidationParameters _validationParameters = new()
    {
        ValidateIssuerSigningKey = false,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        RequireExpirationTime = true
    };


    /// <inheritdoc/>
    public string? Authenticate(string? token)
    {
        try
        {
            string cleanToken = ExtractToken(token);
            JwtSecurityToken validatedToken = ValidateToken(cleanToken);
            return ParseIdFromToken(validatedToken);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Cleans and extracts the actual token from the provided authentication header string.
    /// </summary>
    /// <param name="authHeader">The token as found within the authentication header</param>
    /// <returns>a cleaned token, with the token prefix and redundant whitespace removed.</returns>
    /// <exception cref="ArgumentException">Throws ArgumentException if passed header is Null or Whitespace</exception>
    internal static string ExtractToken(string? authHeader)
    {
        if (string.IsNullOrWhiteSpace(authHeader))
        {
            throw new ArgumentException(Errors.Authentication.NullOrWhitespaceToken, nameof(authHeader));
        }

        string trimmedToken = authHeader.Trim();

        if (!trimmedToken.StartsWith(Auth.BearerTokenPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(Errors.Authentication.InvalidToken, nameof(authHeader));
        }

        return trimmedToken[Auth.BearerTokenPrefix.Length..]
            .TrimStart();
    }

    /// <summary>
    /// Parses and validates the provided JWT token
    /// </summary>
    /// <param name="cleanToken">The cleaned token string</param>
    /// <returns>A valid JwtSecurityToken</returns>
    /// <exception cref="SecurityTokenException">Throws a SecurityTokenException if the token fails validation or is invalid</exception>
    internal JwtSecurityToken ValidateToken(string cleanToken)
    {
        // Validates the token and throw SecurityTokenException if invalid
        _tokenHandler.ValidateToken(cleanToken,
                                    _validationParameters,
                                    out SecurityToken validatedToken);

        JwtSecurityToken jwtSecurityToken = (JwtSecurityToken)validatedToken;

        return jwtSecurityToken;
    }

    /// <summary>
    /// Parses a validated token, first checking that the required claims are present.
    /// As we are accepting multiple providers, we need to provide an Id that includes the provider
    /// to prevent id collisions.
    /// </summary>
    /// <param name="validatedToken">A previously validated JwtSecurityToken</param>
    /// <returns>The custom ID based on the provided token</returns>
    /// <exception cref="ArgumentException">Throws an ArgumentException if any of the required claims are null or whitespace</exception>
    internal static string ParseIdFromToken(JwtSecurityToken validatedToken)
    {
        if (string.IsNullOrWhiteSpace(validatedToken.Issuer) || string.IsNullOrWhiteSpace(validatedToken.Subject))
        {
            throw new ArgumentException(Errors.Authentication.InvalidToken, nameof(validatedToken));
        }
        return $"{validatedToken.Issuer}:{validatedToken.Subject}";
    }
}
