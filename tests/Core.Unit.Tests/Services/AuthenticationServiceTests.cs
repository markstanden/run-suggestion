using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Theory;

namespace RunSuggestion.Core.Unit.Tests.Services;

[TestSubject(typeof(AuthenticationService))]
public class AuthenticationServiceTests
{
    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void ValidateToken_WithInvalidToken_ThrowsException(string invalidToken)
    {
        // Arrange
        const string expectedMessage = Errors.Authentication.NullOrWhitespaceToken;

        // Act
        Func<string?> withInvalidToken = () => AuthenticationService.ValidateToken(invalidToken);

        // Assert 
        ArgumentException ex = withInvalidToken.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain(expectedMessage);
    }

    [Theory]
    [InlineData(Any.FourCharString)]
    [InlineData(Any.FiveCharString)]
    [InlineData(Any.SixCharString)]
    public void ValidateToken_WithInvalidBearerToken_ThrowsArgumentException(string invalidBearerToken)
    {
        // Arrange
        const string expectedMessage = Errors.Authentication.InvalidToken;

        // Act
        Func<string?> withInvalidBearerToken = () => AuthenticationService.ValidateToken(invalidBearerToken);

        // Assert
        ArgumentException ex = withInvalidBearerToken.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain(expectedMessage);
    }
}
