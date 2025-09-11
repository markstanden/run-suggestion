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
    private readonly Mock<ILogger<RecommendationService>> _mockLogger = new();
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly RecommendationService _sut;

    public RecommendationServiceCalculateDistanceTests()
    {
        _sut = new RecommendationService(_mockLogger.Object, _mockRepository.Object, _currentDate);
    }

    [Theory]
    [InlineData(10000, 3, 5, 11500)]
    [InlineData(10000, 3, 10, 13000)]
    [InlineData(10000, 3, 15, 14500)]
    [InlineData(10000, 3, 20, 16000)]
    [InlineData(20000, 3, 10, 26000)]
    public void CalculateDistance_WithOneRemainingRunAndProvidedPercentageProgression_ProvidesExpectedDistance(
        int runDistance,
        int weeklyRuns,
        int percentageProgression,
        int expectedDistance)
    {
        // Arrange
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
    [InlineData(10000, 3, 5, 1500)]
    [InlineData(10000, 3, 10, 3000)]
    [InlineData(10000, 3, 15, 4500)]
    [InlineData(10000, 3, 20, 6000)]
    [InlineData(20000, 3, 10, 6000)]
    public void CalculateDistance_WithConsistentWeeklyRunning_ShouldSuggestWeeklyOffset(
        int runDistance,
        int weeklyRuns,
        int percentageProgression,
        int expectedDistance)
    {
        // Arrange
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
