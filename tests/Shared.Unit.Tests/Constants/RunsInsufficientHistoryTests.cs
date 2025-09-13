using JetBrains.Annotations;
using RunSuggestion.Shared.Constants;
using Shouldly;

namespace RunSuggestion.Shared.Unit.Tests.Constants;

[TestSubject(typeof(Runs.InsufficientHistory))]
public class RunsInsufficientHistoryTests
{
    [Fact]
    public void RunDurationTimeSpan_WithZeroDistance_ReturnsRestDuration()
    {
        // Arrange
        const int restDistance = Runs.RestDistance;

        // Act
        TimeSpan result = Runs.InsufficientHistory.RunDurationTimeSpan(restDistance);

        // Assert
        result.ShouldBe(Runs.RestDuration);
    }

    [Theory]
    [InlineData(1000, 15, 15)]
    [InlineData(5000, 15, 75)]
    [InlineData(10000, 15, 150)]
    public void RunDurationTimeSpan_WithValidDistance_CalculatesCorrectDuration(
        int distanceMetres,
        int paceMinsPerKm,
        int expectedMinutes)
    {
        // Arrange
        TimeSpan expected = TimeSpan.FromMinutes(expectedMinutes);

        // Act
        TimeSpan result = Runs.InsufficientHistory.RunDurationTimeSpan(distanceMetres, paceMinsPerKm);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-1000)]
    [InlineData(-10000)]
    public void RunDurationTimeSpan_WithNegativeDistance_ThrowsArgumentOutOfRangeException(int negativeDistance)
    {
        // Act
        Action actionWithNegativeDistance = () => Runs.InsufficientHistory.RunDurationTimeSpan(negativeDistance);

        // Assert
        ArgumentOutOfRangeException ex = actionWithNegativeDistance.ShouldThrow<ArgumentOutOfRangeException>();
        ex.ParamName.ShouldBe("distanceMetres");
        ex.Message.ShouldContain("Distance must be a positive integer");
        ex.Message.ShouldContain("cannot be negative");
    }
}
