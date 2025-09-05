using System.Text.Json;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Auth;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Creators;
using RunSuggestion.TestHelpers.Theory;

namespace RunSuggestion.Core.Unit.Tests.Services.SwaAuthenticationServiceTests;

[TestSubject(typeof(JwtAuthenticationService))]
public class ExtractTokenTests
{
    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void ExtractJson_WithInvalidToken_ThrowsArgumentException(string invalidToken)
    {
        // Arrange
        const string expectedMessage = Errors.Authentication.NullOrWhitespaceToken;
        const string expectedParamName = "jsonToken";

        // Act
        Func<string?> withInvalidToken = () => SwaAuthenticationService.ExtractJson(invalidToken);

        // Assert 
        ArgumentException ex = withInvalidToken.ShouldThrow<ArgumentException>();
        ex.ParamName.ShouldBe(expectedParamName);
        ex.Message.ShouldContain(expectedMessage);
    }

    [Theory]
    [InlineData("   ", "")]
    [InlineData("", "   ")]
    [InlineData(" ", "")]
    [InlineData("", " ")]
    [InlineData("\r", "")]
    [InlineData("", "\r")]
    [InlineData("\n", "")]
    [InlineData("", "\n")]
    [InlineData("\r\n", "")]
    [InlineData("", "\r\n")]
    [InlineData("  \r\n  ", "  \r\n  ")]
    public void ExtractJson_WithValidBearerToken_ForgivesLeadingAndTrailingWhitespace(
        string? prefix, string suffix)
    {
        // Arrange
        SwaClientPrincipal principal = AuthFakes.CreateFakeClientPrincipal();
        string expectedPrincipalJson = JsonSerializer.Serialize(principal);
        string encodedToken = AuthFakes.EncodePrincipalToTokenString(principal);
        string headerValue = $"{prefix}{encodedToken}{suffix}";

        // Act
        string result = SwaAuthenticationService.ExtractJson(headerValue);

        // Assert
        result.ShouldBe(expectedPrincipalJson);
    }

    [Theory]
    [InlineData(Any.LongAlphanumericString, Any.FourCharString, Any.ShortAlphanumericString)]
    [InlineData(Any.ShortAlphanumericString, Any.FiveCharString, Any.LongAlphanumericString)]
    [InlineData(Any.String, Any.SixCharString, Any.LongAlphaWithSpecialCharsString)]
    public void ExtractJson_WithValidBearerToken_ReturnsExpectedPrincipalJson(
        string userId, string provider, string? userDetails)
    {
        // Arrange
        SwaClientPrincipal principal = AuthFakes.CreateFakeClientPrincipal(userId, provider, userDetails);
        string expectedPrincipalJson = JsonSerializer.Serialize(principal);
        string encodedToken = AuthFakes.EncodePrincipalToTokenString(principal);

        // Act
        string result = SwaAuthenticationService.ExtractJson(encodedToken);

        // Assert
        result.ShouldBe(expectedPrincipalJson);
    }
}
