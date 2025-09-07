using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Auth;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Creators;
using RunSuggestion.TestHelpers.Theory;

namespace RunSuggestion.Core.Unit.Tests.Services.SwaAuthenticationServiceTests;

[TestSubject(typeof(SwaAuthenticationService))]
public class AuthenticateTests
{
    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void Authenticate_WithInvalidToken_ReturnsNull(string invalidToken)
    {
        // Arrange
        SwaAuthenticationService sut = new();

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
        SwaAuthenticationService sut = new();

        // Act
        string? result = sut.Authenticate(invalidBearerToken);

        // Assert 
        result.ShouldBeNull();
    }

    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void Authenticate_WithInvalidUserIdWithinHeader_ReturnsNull(string userId)
    {
        // Arrange
        string headerValue = AuthFakes.EncodePrincipalToTokenString(new SwaClientPrincipal
        {
            UserId = userId,
            IdentityProvider = Auth.ValidIssuers.First(),
            UserDetails = Any.LongAlphanumericString
        });
        SwaAuthenticationService sut = new();

        // Act
        string? result = sut.Authenticate(headerValue);

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData(Any.ShortAlphanumericString)]
    [InlineData(Any.LongAlphanumericString)]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void Authenticate_WithInvalidProvider_ReturnsNull(string provider)
    {
        // Arrange
        string headerValue = AuthFakes.EncodePrincipalToTokenString(new SwaClientPrincipal
        {
            UserId = Any.LongAlphanumericString,
            IdentityProvider = provider,
            UserDetails = Any.LongAlphanumericString
        });
        SwaAuthenticationService sut = new();

        // Act
        string? result = sut.Authenticate(headerValue);

        // Assert
        result.ShouldBeNull();
    }

    [Theory]
    [InlineData(Any.LongAlphanumericString, Auth.Issuers.GitHub)]
    public void Authenticate_WithValidAuthHeader_ReturnsExpectedId(string userId, string provider)
    {
        // Arrange
        string expectedEntraId = $"{provider}:{userId}";
        string headerValue = AuthFakes.CreateFakeBase64SwaClientPrincipal(userId, provider);
        SwaAuthenticationService sut = new();

        // Act
        string? result = sut.Authenticate(headerValue);

        // Assert
        Auth.ValidIssuers.ShouldContain(provider);
        result.ShouldNotBeNull();
        result.ShouldBe(expectedEntraId);
    }
}
