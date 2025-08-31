using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.TestHelpers;

namespace RunSuggestion.Core.Unit.Tests.Services;

public class AuthenticationServiceValidateTokenTests
{
    private readonly AuthenticationService _sut;

    public AuthenticationServiceValidateTokenTests()
    {
        _sut = new AuthenticationService();
    }

    private static JwtSecurityToken CreateFakeJwtToken(
        string? issuer = null,
        string? subject = null)
    {
        List<Claim> claims = new();

        if (subject is not null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subject));
        }

        return new JwtSecurityToken(
            issuer: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5)
        );
    }

    [Theory]
    [InlineData(Auth.Issuers.GitHub)]
    public void ValidateToken_WithValidToken_ReturnsJwtSecurityToken(string validIssuer)

    {
        // Arrange
        JwtSecurityToken token = CreateFakeJwtToken(
            issuer: validIssuer,
            Any.LongAlphanumericString);
        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Act
        JwtSecurityToken? result = _sut.ValidateToken(tokenString);

        // Assert
        result.ShouldNotBeNull();
        result.Issuer.ShouldBe(validIssuer);
    }

    [Theory]
    [InlineData(Any.FourCharString)]
    [InlineData(Any.FiveCharString)]
    [InlineData(Any.SixCharString)]
    public void ValidateToken_WithValidTokenFromUnsupportedIssuer_ShouldThrowSecurityException(
        string unsupportedIssuer)
    {
        // Arrange
        JwtSecurityToken token = CreateFakeJwtToken(
            issuer: unsupportedIssuer,
            Any.LongAlphanumericString);
        string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Act
        Func<JwtSecurityToken?> withUnsupportedProvider = () => _sut.ValidateToken(tokenString);

        // Assert
        Exception ex = withUnsupportedProvider.ShouldThrow<SecurityTokenException>();
        ex.Message.ShouldContain("Issuer validation failed");
    }
}
