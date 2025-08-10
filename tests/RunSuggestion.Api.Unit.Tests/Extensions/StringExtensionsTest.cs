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
}
