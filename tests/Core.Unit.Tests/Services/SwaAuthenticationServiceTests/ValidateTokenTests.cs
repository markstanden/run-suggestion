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
        // Arrange
        string encodedToken = Base64UrlEncoder.Encode(invalidJson);

        // Act
        Func<SwaClientPrincipal> withInvalidJson = () => SwaAuthenticationService.ValidateToken(encodedToken);

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
        SwaClientPrincipal principal = AuthFakes.CreateFakeClientPrincipal(invalidUserId);
        string encodedToken = AuthFakes.EncodePrincipalToTokenString(principal);

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
        SwaClientPrincipal principal = AuthFakes.CreateFakeClientPrincipal(identityProvider: invalidIdentityProvider);
        string encodedToken = AuthFakes.EncodePrincipalToTokenString(principal);

        // Act
        Func<SwaClientPrincipal> withInvalidUserId = () => SwaAuthenticationService.ValidateToken(encodedToken);

        // Assert
        Exception ex = withInvalidUserId.ShouldThrow<SecurityTokenException>();
        ex.Message.ShouldContain(Errors.Authentication.InvalidToken);
    }

    [Theory]
    [InlineData(Any.String, Auth.Issuers.GitHub)]
    [InlineData(Any.LongAlphanumericString, Auth.Issuers.GitHub)]
    public void ValidateToken_WithValidUserIdAndProvider_ReturnsExpectedPrincipal(
        string validUserId, string validIdentityProvider)
    {
        // Arrange
        string encodedToken = AuthFakes.CreateFakeBase64SwaClientPrincipal(validUserId, validIdentityProvider);

        // Act
        SwaClientPrincipal result = SwaAuthenticationService.ValidateToken(encodedToken);

        // Assert
        result.UserId.ShouldBe(validUserId);
        result.IdentityProvider.ShouldBe(validIdentityProvider);
    }
}
