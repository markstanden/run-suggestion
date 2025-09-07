using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Auth;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Creators;
using RunSuggestion.TestHelpers.Theory;

namespace RunSuggestion.Core.Unit.Tests.Services.SwaAuthenticationServiceTests;

public class ValidateTokenTests
{
    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void ValidateToken_WithInvalidToken_ThrowsArgumentException(string invalidToken)
    {
        // Arrange
        const string expectedMessage = Errors.Authentication.NullOrWhitespaceToken;
        const string expectedParamName = "jsonPrincipal";

        // Act
        Func<SwaClientPrincipal?> withInvalidToken = () => SwaAuthenticationService.ValidateToken(invalidToken);

        // Assert 
        ArgumentException ex = withInvalidToken.ShouldThrow<ArgumentException>();
        ex.ParamName.ShouldBe(expectedParamName);
        ex.Message.ShouldContain(expectedMessage);
    }

    [Theory]
    [InlineData(Any.String)]
    [InlineData(Any.ShortAlphanumericString)]
    [InlineData(Any.LongAlphanumericString)]
    public void ValidateToken_WithInvalidJson_ThrowsSecurityTokenException(
        string invalidJson)
    {
        // Act
        Func<SwaClientPrincipal> withInvalidJson = () => SwaAuthenticationService.ValidateToken(invalidJson);

        // Assert
        Exception ex = withInvalidJson.ShouldThrow<SecurityTokenException>();
        ex.Message.ShouldContain(Errors.Authentication.InvalidToken);
    }

    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void ValidateToken_WithInvalidUserId_ThrowsSecurityTokenException(
        string invalidUserId)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = invalidUserId,
            IdentityProvider = Auth.ValidIssuers.First(),
            UserDetails = Any.String
        };
        string encodedToken = JsonSerializer.Serialize(principal);

        // Act
        Func<SwaClientPrincipal> withInvalidUserId = () => SwaAuthenticationService.ValidateToken(encodedToken);

        // Assert
        Exception ex = withInvalidUserId.ShouldThrow<SecurityTokenException>();
        ex.Message.ShouldContain(Errors.Authentication.InvalidToken);
    }

    [Theory]
    [InlineData(Any.String)]
    [InlineData(Any.LongAlphanumericString)]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void ValidateToken_WithInvalidProvider_ThrowsSecurityTokenException(
        string invalidIdentityProvider)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = Any.String,
            IdentityProvider = invalidIdentityProvider,
            UserDetails = Any.String
        };
        string encodedToken = JsonSerializer.Serialize(principal);

        // Act
        Func<SwaClientPrincipal> withInvalidUserId = () => SwaAuthenticationService.ValidateToken(encodedToken);

        // Assert
        Exception ex = withInvalidUserId.ShouldThrow<SecurityTokenException>();
        ex.Message.ShouldContain(Errors.Authentication.InvalidToken);
    }

    [Theory]
    [InlineData(Any.String)]
    [InlineData(Any.LongAlphanumericString)]
    public void ValidateToken_WithValidUserIdAndProvider_ReturnsExpectedPrincipal(string validUserId)
    {
        // Arrange
        string validIdentityProvider = Auth.ValidIssuers.First();
        SwaClientPrincipal principal = AuthFakes.CreateFakeClientPrincipal(validUserId, validIdentityProvider);
        string encodedToken = JsonSerializer.Serialize(principal);

        // Act
        SwaClientPrincipal result = SwaAuthenticationService.ValidateToken(encodedToken);

        // Assert
        result.UserId.ShouldBe(validUserId);
        result.IdentityProvider.ShouldBe(validIdentityProvider);
    }
}
