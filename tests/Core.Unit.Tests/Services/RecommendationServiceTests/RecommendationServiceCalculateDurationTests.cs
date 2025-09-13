using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.TestHelpers.Creators;
using static RunSuggestion.Shared.Constants.Runs.EffortLevel;

namespace RunSuggestion.Core.Unit.Tests.Services.RecommendationServiceTests;

[TestSubject(typeof(RecommendationService))]
public class RecommendationServiceCalculateDurationTests
{
    private readonly DateTime _currentDate = new(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ILogger<RecommendationService>> _mockLogger = new();
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly RecommendationService _sut;

    public RecommendationServiceCalculateDurationTests()
    {
        _sut = new RecommendationService(_mockLogger.Object, _mockRepository.Object, _currentDate);
    }

    [Fact]
    public void CalculateDuration_WithEmptyRunHistory_ReturnsDefaultDuration()
    {
        // Arrange
        int distanceMetres = Runs.InsufficientHistory.RunDistanceMetres;
        TimeSpan expectedTimeSpan = Runs.InsufficientHistory.RunDurationTimeSpan(distanceMetres);
        IEnumerable<RunEvent> runEvents = [];

        // Act
        TimeSpan result = _sut.CalculateDuration(runEvents, distanceMetres, Any.Byte);

        // Assert
        result.ShouldBe(expectedTimeSpan);
    }

    [Theory]
    [InlineData(5, 25)]
    [InlineData(10, 50)]
    [InlineData(15, 75)]
    [InlineData(20, 100)]
    public void CalculateDuration_WithSingleEffortLevelAndConsistentPace_CalculatesTimeFromRecommendedDistance(
        int recommendedDistanceKm, int expectedDurationMins)
    {
        // Arrange
        byte consistentEffortLevel = Easy;
        int paceMinsPerKm = 5;
        int historicDistances = Any.Integer;
        IEnumerable<RunEvent> runEvents = Enumerable.Range(1, 28)
            .Select(day => RunBaseFakes.CreateRunEventWithPace(historicDistances,
                                                               paceMinsPerKm,
                                                               consistentEffortLevel,
                                                               _currentDate.AddDays(-day)));
        TimeSpan expectedDuration = TimeSpan.FromMinutes(expectedDurationMins);

        // Act
        TimeSpan result = _sut.CalculateDuration(runEvents, recommendedDistanceKm * 1000, consistentEffortLevel);

        // Assert
        result.ShouldBe(expectedDuration);
    }

    [Theory]
    [InlineData(new[] { 5, 6, 7, 8, 9 }, 10, 70)]
    [InlineData(new[] { 6, 7, 8, 9, 10 }, 10, 80)]
    [InlineData(new[] { 7, 8, 9, 10, 11 }, 10, 90)]
    [InlineData(new[] { 8, 9, 10, 11, 12 }, 10, 100)]
    [InlineData(new[] { 9, 10, 11, 12, 13 }, 10, 110)]
    public void CalculateDuration_WithSingleEffortLevelAndVariedPace_CalculatesTimeFromRecommendedDistance(
        int[] paceMinsPerKm, int recommendedDistanceKm, int expectedDurationMins)
    {
        // Arrange
        byte consistentEffortLevel = Easy;
        int historicDistances = Any.Integer;
        IEnumerable<RunEvent> runEvents = paceMinsPerKm
            .Select((pace, index) => RunBaseFakes.CreateRunEventWithPace(historicDistances,
                                                                         pace,
                                                                         consistentEffortLevel,
                                                                         _currentDate.AddDays(-(index + 1))));
        TimeSpan expectedDuration = TimeSpan.FromMinutes(expectedDurationMins);

        // Act
        TimeSpan result = _sut.CalculateDuration(runEvents, recommendedDistanceKm * 1000, consistentEffortLevel);

        // Assert
        result.ShouldBe(expectedDuration);
    }

    [Theory]
    [InlineData(Recovery, 10, 120)]
    [InlineData(Recovery, 20, 240)]
    [InlineData(Easy, 10, 100)]
    [InlineData(Easy, 15, 150)]
    [InlineData(Strong, 5, 30)]
    [InlineData(Strong, 10, 60)]
    [InlineData(Hard, 6, 24)]
    [InlineData(Hard, 4, 16)]
    public void CalculateDuration_WithMultipleEffortLevels_CalculatesTimeFromRecommendedDistanceUsingCorrectEffortLevel(
        byte consistentEffortLevel, int distanceKm, int expectedDurationMins)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = Enumerable
            .Range(1, 10)
            .Select(i => 4 * i)
            .SelectMany<int, RunEvent>(offset =>
            [
                RunBaseFakes.CreateRunEventWithPace(10, 12, Recovery, _currentDate.AddDays(-1 - offset)),
                RunBaseFakes.CreateRunEventWithPace(15, 10, Easy, _currentDate.AddDays(-2 - offset)),
                RunBaseFakes.CreateRunEventWithPace(5, 6, Strong, _currentDate.AddDays(-3 - offset)),
                RunBaseFakes.CreateRunEventWithPace(2, 4, Hard, _currentDate.AddDays(-4 - offset))
            ]);
        TimeSpan expectedDuration = TimeSpan.FromMinutes(expectedDurationMins);

        // Act
        TimeSpan result = _sut.CalculateDuration(runEvents, distanceKm * 1000, consistentEffortLevel);

        // Assert
        result.ShouldBe(expectedDuration);
    }
}
