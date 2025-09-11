using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Core.Unit.Tests.Services.RecommendationServiceTests;

[TestSubject(typeof(RecommendationService))]
public class RecommendationServiceCalculateDistanceTests
{
    private readonly DateTime _currentDate = new(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc);
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly Mock<ILogger<RecommendationService>> _mockLogger = new();
    private readonly RecommendationService _sut;

    public RecommendationServiceCalculateDistanceTests()
    {
        _sut = new RecommendationService(_mockLogger.Object, _mockRepository.Object, _currentDate);
    }

    [Theory]
    [InlineData(2500, 5, 2625)]
    [InlineData(5000, 5, 5250)]
    [InlineData(10000, 5, 10500)]
    [InlineData(15000, 5, 15750)]
    [InlineData(20000, 10, 22000)]
    public void CalculateDistance_WithOneRemainingRunAndProvidedPercentageProgression_ProvidesExpectedDistance(
        int runDistance,
        int percentageProgression,
        int expectedDistance)
    {
        // Arrange
        int weeklyRuns = 3;
        int numberOfWeeks = 4;
        IEnumerable<RunEvent> runEvents = Enumerable.Range(0, numberOfWeeks)
            .SelectMany(weekNumber => RunBaseFakes.CreateWeekOfRuns(runDistance,
                                                                    _currentDate.AddDays(-7 * weekNumber),
                                                                    weeklyRuns))
            .OrderBy(run => run.Date)
            .SkipLast(1);

        // Act
        int result = _sut.CalculateDistance(runEvents, percentageProgression);

        // Assert
        result.ShouldBe(expectedDistance);
    }

    [Theory]
    [InlineData(2500, 5, 125)]
    [InlineData(5000, 5, 250)]
    [InlineData(10000, 5, 500)]
    [InlineData(15000, 5, 750)]
    [InlineData(20000, 10, 2000)]
    public void CalculateDistance_WithConsistentWeeklyRunning_ShouldSuggestOffset(
        int runDistance,
        int percentageProgression,
        int expectedDistance)
    {
        // Arrange
        int weeklyRuns = 3;
        int numberOfWeeks = 4;
        IEnumerable<RunEvent> runEvents = Enumerable.Range(0, numberOfWeeks)
            .SelectMany(weekNumber => RunBaseFakes.CreateWeekOfRuns(runDistance,
                                                                    _currentDate.AddDays(-7 * weekNumber),
                                                                    weeklyRuns));

        // Act
        int result = _sut.CalculateDistance(runEvents, percentageProgression);

        // Assert
        result.ShouldBe(expectedDistance);
    }
}
