using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Theory;

namespace RunSuggestion.Core.Unit.Tests.Services;

[TestSubject(typeof(AuthenticationService))]
public class AuthenticationServiceExtractTokenTests
{
    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void ExtractToken_WithInvalidToken_ThrowsException(string invalidToken)
    {
        // Arrange
        const string expectedMessage = Errors.Authentication.NullOrWhitespaceToken;

        // Act
        Func<string?> withInvalidToken = () => AuthenticationService.ExtractToken(invalidToken);

        // Assert 
        ArgumentException ex = withInvalidToken.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain(expectedMessage);
    }

    [Theory]
    [InlineData(Any.FourCharString)]
    [InlineData(Any.FiveCharString)]
    [InlineData(Any.SixCharString)]
    public void ExtractToken_WithInvalidBearerToken_ThrowsArgumentException(string invalidBearerToken)
    {
        // Arrange
        const string expectedMessage = Errors.Authentication.InvalidToken;

        // Act
        Func<string?> withInvalidBearerToken = () => AuthenticationService.ExtractToken(invalidBearerToken);

        // Assert
        ArgumentException ex = withInvalidBearerToken.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain(expectedMessage);
    }

    [Theory]
    [InlineData(Any.FourCharString)]
    [InlineData(Any.FiveCharString)]
    [InlineData(Any.LongAlphanumericString)]
    public void ExtractToken_WithValidBearerToken_ReturnsExtractedTokenString(string validBearerToken)
    {
        // Arrange
        string headerValue = $"{Auth.BearerTokenPrefix} {validBearerToken}";

        // Act
        string result = AuthenticationService.ExtractToken(headerValue);

        // Assert
        result.ShouldBe(validBearerToken);
    }

    [Theory]
    [InlineData($"{Any.LongAlphanumericString}")]
    [InlineData($"    {Any.LongAlphanumericString}")]
    [InlineData($"{Any.LongAlphanumericString}     ")]
    [InlineData($"    {Any.LongAlphanumericString}    ")]
    [InlineData($"{Any.LongAlphanumericString}\n")]
    [InlineData($"{Any.LongAlphanumericString}\r")]
    [InlineData($"{Any.LongAlphanumericString}\r\n")]
    public void ExtractToken_WithValidBearerToken_ForgivesLeadingAndTrailingWhitespace(
        string? validBearerTokenWithWhitespace)
    {
        // Arrange
        const string validBearerToken = Any.LongAlphanumericString;
        string headerValue = $"    {Auth.BearerTokenPrefix} {validBearerTokenWithWhitespace}    ";

        // Act
        string result = AuthenticationService.ExtractToken(headerValue);

        // Assert
        result.ShouldBe(validBearerToken);
    }
}
