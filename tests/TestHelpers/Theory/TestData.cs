using Xunit;

namespace RunSuggestion.TestHelpers.Theory;

public static class TestData
{
    /// <summary>
    /// Theory data for null, empty, and whitespace strings
    /// </summary>
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
