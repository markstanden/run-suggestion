using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Theory;

namespace RunSuggestion.Core.Unit.Tests.Services;

[TestSubject(typeof(AuthenticationService))]
public class AuthenticationServiceAuthenticateTests
{
    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void Authenticate_WithInvalidToken_ReturnsNull(string invalidToken)
    {
        // Arrange
        AuthenticationService sut = new();

        // Act
        string? result = sut.Authenticate(invalidToken);

        // Assert 
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData(Any.FourCharString)]
    [InlineData(Any.FiveCharString)]
    [InlineData(Any.LongAlphanumericString)]
    public void Authenticate_WithInvalidBearerToken_ReturnsNull(string invalidBearerToken)
    {
        // Arrange
        AuthenticationService sut = new();

        // Act
        string? result = sut.Authenticate(invalidBearerToken);

        // Assert 
        result.ShouldBeNull();
    }

    /*
    [Theory]
    [InlineData(Any.LongAlphanumericString, "")]
    public void Authenticate_WithValidBearerToken_ReturnsExtractedTokenString(string validBearerToken,
        string? expectedEntraId)
    {
        // Arrange
        string headerValue = $"{Auth.BearerTokenPrefix} {validBearerToken}";
        AuthenticationService sut = new();

        // Act
        string? result = sut.Authenticate(headerValue);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedEntraId);
    }
    */
}
