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
    public static readonly TheoryData<string> NullOrWhitespace =
    [
        null!,
        "",
        "    ",
        "\n",
        "\r",
        "\r\n",
        "\t"
    ];


    public static TheoryData<int[]> VaryingWeeklyLoadsTestData =>
    [
        FourWeekRunLoads.FiveKmAverage.NoVariance,
        FourWeekRunLoads.FiveKmAverage.LowVariance,
        FourWeekRunLoads.FiveKmAverage.HighVariance,
        FourWeekRunLoads.FiveKmAverage.Progression,
        FourWeekRunLoads.FiveKmAverage.Decreasing,

        FourWeekRunLoads.TenKmAverage.NoVariance,
        FourWeekRunLoads.TenKmAverage.LowVariance,
        FourWeekRunLoads.TenKmAverage.HighVariance,
        FourWeekRunLoads.TenKmAverage.Progression,
        FourWeekRunLoads.TenKmAverage.Decreasing,

        FourWeekRunLoads.TwentyKmAverage.NoVariance,
        FourWeekRunLoads.TwentyKmAverage.LowVariance,
        FourWeekRunLoads.TwentyKmAverage.HighVariance,
        FourWeekRunLoads.TwentyKmAverage.Progression,
        FourWeekRunLoads.TwentyKmAverage.Decreasing
    ];


    public static class FourWeekRunLoads
    {
        public static class FiveKmAverage
        {
            public static readonly int[] NoVariance = [5000, 5000, 5000, 5000, 5000];
            public static readonly int[] LowVariance = [5000, 6000, 4000, 5500, 4500];
            public static readonly int[] HighVariance = [5000, 7500, 2500, 7000, 3000];
            public static readonly int[] Progression = [5000, 5500, 5250, 4750, 4500];
            public static readonly int[] Decreasing = [5000, 4500, 4750, 5250, 5500];
        }

        public static class TenKmAverage
        {
            public static readonly int[] NoVariance = [10000, 10000, 10000, 10000, 10000];
            public static readonly int[] LowVariance = [10000, 12000, 8000, 11000, 9000];
            public static readonly int[] HighVariance = [10000, 15000, 5000, 14000, 6000];
            public static readonly int[] Progression = [10000, 11000, 10500, 9500, 9000];
            public static readonly int[] Decreasing = [10000, 9000, 9500, 10500, 11000];
        }

        public static class TwentyKmAverage
        {
            public static readonly int[] NoVariance = [20000, 20000, 20000, 20000, 20000];
            public static readonly int[] LowVariance = [20000, 24000, 16000, 22000, 18000];
            public static readonly int[] HighVariance = [20000, 30000, 10000, 28000, 12000];
            public static readonly int[] Progression = [20000, 22000, 21000, 19000, 18000];
            public static readonly int[] Decreasing = [20000, 18000, 19000, 21000, 22000];
        }
    }
}
