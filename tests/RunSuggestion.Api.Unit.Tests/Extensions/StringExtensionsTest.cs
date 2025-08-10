using RunSuggestion.Api.Extensions;

namespace RunSuggestion.Api.Unit.Tests.Extensions;

[TestSubject(typeof(StringExtensions))]
public class StringExtensionsTest
{
    [Theory]
    [InlineData("ABCDEF", "BCDEF")]
    [InlineData("0123456789", "56789")]
    [InlineData("ABCDEF12345", "12345")]
    [InlineData("1 2 3 4 5", "3 4 5")]
    public void LastFiveChars_WithAStringOfMoreThanFiveChars_ReturnsLastFiveChars(string initial, string expected)
    {
        // Act
        string actual = initial.LastFiveChars();

        // Assert
        initial.Length.ShouldBeGreaterThan(5);
        actual.Length.ShouldBe(5);
        actual.ShouldBe(expected);
    }


    [Theory]
    [InlineData("AB 12")]
    [InlineData("ABCDE")]
    [InlineData("ABC")]
    [InlineData("A")]
    [InlineData("")]
    public void LastFiveChars_WithAStringOfFiveCharsOrLess_ReturnsInitialString(string initial)
    {
        // Act
        string actual = initial.LastFiveChars();

        // Assert
        initial.Length.ShouldBeLessThanOrEqualTo(5);
        actual.ShouldBe(initial);
        actual.ShouldBeSameAs(initial);
    }

    [Fact]
    public void LastFiveChars_WithNullString_ThrowsArgumentNullException()
    {
        // Arrange
        string? nullString = null;

        // Act
        Action withNullStringArgument = () => nullString!.LastFiveChars();

        // Assert
        withNullStringArgument.ShouldThrow<ArgumentNullException>();
    }
}
