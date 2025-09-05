using Xunit;

namespace RunSuggestion.TestHelpers.Theory;

public static class TestData
{
    /// <summary>
    /// Theory data for null, empty, and whitespace strings.
    /// Used to test null, empty, and whitespace input validation.
    /// </summary>
    /// <example>
    /// <code>
    /// [Theory]
    /// [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    /// public void Method_WithEmptyInputString_DoesSomething(string whitespaceString)
    /// {
    ///     // Arrange
    ///     // Act
    ///     // Assert
    /// }
    /// </code> 
    /// </example>
    public static readonly TheoryData<string> NullOrWhitespace = new()
    {
        null!,
        "",
        "    ",
        "\n",
        "\r",
        "\r\n",
        "\t"
    };
}
