using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.TestHelpers.Theory;

namespace RunSuggestion.Core.Unit.Tests.Services;

public class AuthenticationServiceTests
{
    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public void Authenticate_WithInvalidToken_ThrowsException(string invalidToken)
    {
        // Arrange
        AuthenticationService sut = new();

        // Act
        Func<string?> withInvalidToken = () => sut.Authenticate(invalidToken);

        // Assert
        ArgumentException ex = withInvalidToken.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain(Errors.Authentication.NullOrWhitespaceToken);
    }
}
