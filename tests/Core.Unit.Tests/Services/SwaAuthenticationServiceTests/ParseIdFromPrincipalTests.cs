using Microsoft.IdentityModel.Tokens;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Auth;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Creators;
using RunSuggestion.TestHelpers.Theory;

namespace RunSuggestion.Core.Unit.Tests.Services.SwaAuthenticationServiceTests;

public class ParseIdFromPrincipalTests
{
    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void ParseIdFromPrincipal_WithEmptyUserId_ThrowsSecurityTokenException(
        string invalidUserId)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = invalidUserId,
            IdentityProvider = Auth.ValidIssuers.First()
        };

        // Act
        Func<string> withEmptyUserId = () => SwaAuthenticationService.ParseIdFromPrincipal(principal);

        // Assert
        Exception ex = withEmptyUserId.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain(Errors.Authentication.InvalidToken);
    }

    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void ParseIdFromPrincipal_WithEmptyProvider_ThrowsSecurityTokenException(
        string invalidIdentityProvider)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = Any.LongAlphanumericString,
            IdentityProvider = invalidIdentityProvider
        };

        // Act
        Func<string> withEmptyIdentityProvider = () => SwaAuthenticationService.ParseIdFromPrincipal(principal);

        // Assert
        Exception ex = withEmptyIdentityProvider.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain(Errors.Authentication.InvalidToken);
    }

    [Theory]
    [InlineData(Any.String, Auth.Issuers.GitHub)]
    [InlineData(Any.LongAlphanumericString, Auth.Issuers.GitHub)]
    public void ParseIdFromPrincipal_WithValidUserIdAndProvider_ShouldReturnStringContainingBothUserIdAndProvider(
        string validUserId, string validIdentityProvider)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = validUserId,
            IdentityProvider = validIdentityProvider
        };

        // Act
        string result = SwaAuthenticationService.ParseIdFromPrincipal(principal);

        // Assert
        result.ShouldContain(validUserId);
        result.ShouldContain(validIdentityProvider);
    }
}
