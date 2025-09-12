using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Core.Unit.Tests.Services.RecommendationServiceTests;

[TestSubject(typeof(RecommendationService))]
public class RecommendationServiceCalculateEffortTests
{
    private readonly DateTime _currentDate = new(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ILogger<RecommendationService>> _mockLogger = new();
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly RecommendationService _sut;

    public RecommendationServiceCalculateEffortTests()
    {
        _sut = new RecommendationService(_mockLogger.Object, _mockRepository.Object, _currentDate);
    }

    [Theory]
    [InlineData(new byte[] { 1, 1, 1, 1 }, 20, 4)]
    [InlineData(new byte[] { 2, 1, 2, 1 }, 20, 4)]
    [InlineData(new byte[] { 2, 2, 2, 2 }, 20, 4)]
    [InlineData(new byte[] { 1, 1, 1 }, 25, 4)]
    [InlineData(new byte[] { 2, 2, 2 }, 25, 4)]
    public void CalculateEffort_WithMostRunsAtLowEffort_ReturnsHighEffort(byte[] currentWeeklyEffort, int targetEffort,
        byte minSuggestedEffort)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBeGreaterThanOrEqualTo(minSuggestedEffort);
    }
}
