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
    [InlineData(new byte[] { 1, 1, 1, 1 }, 20)]
    [InlineData(new byte[] { 1, 1, 1 }, 25)]
    public void CalculateEffort_WithAnyRunHistory_ReturnsMaxStrongEffort(byte[] currentWeeklyEffort,
        int targetEffort)
    {
        // Arrange
        const byte expectedStrongEffortLevel = 4;
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBeLessThanOrEqualTo(expectedStrongEffortLevel);
    }

    [Theory]
    [InlineData(new byte[] { 1, 1, 1, 1 }, 20)]
    [InlineData(new byte[] { 2, 1, 2, 1 }, 20)]
    [InlineData(new byte[] { 2, 2, 2, 2 }, 20)]
    [InlineData(new byte[] { 1, 1, 1 }, 25)]
    [InlineData(new byte[] { 2, 2, 2 }, 25)]
    public void CalculateEffort_WithMostRunsAtLowEffort_ReturnsStrongEffort(byte[] currentWeeklyEffort,
        int targetEffort)
    {
        // Arrange
        const byte expectedStrongEffortLevel = 4;
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBe(expectedStrongEffortLevel);
    }

    [Theory]
    [InlineData(new byte[] { 4, 1, 1, 1 }, 20)]
    [InlineData(new byte[] { 4, 1, 2, 1 }, 20)]
    [InlineData(new byte[] { 4, 2, 2, 2 }, 20)]
    [InlineData(new byte[] { 4, 1, 1 }, 25)]
    [InlineData(new byte[] { 4, 2, 2 }, 25)]
    public void CalculateEffort_WithRunsAlreadyMeetingTargetEffortPercentage_ReturnsEasyEffort(
        byte[] currentWeeklyEffort, int targetEffort)
    {
        // Arrange
        const byte expectedEasyEffortLevel = 2;
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBe(expectedEasyEffortLevel);
    }

    [Theory]
    [InlineData(new byte[] { 4, 1, 3, 1 }, 20)]
    [InlineData(new byte[] { 4, 2, 3, 1 }, 20)]
    [InlineData(new byte[] { 4, 1, 3, 2 }, 20)]
    [InlineData(new byte[] { 4, 2, 3, 2 }, 20)]
    [InlineData(new byte[] { 4, 1, 3 }, 25)]
    [InlineData(new byte[] { 4, 2, 3 }, 25)]
    public void CalculateEffort_WithRunsAlreadyExceedingTargetEffortPercentage_ReturnsRecoveryEffort(
        byte[] currentWeeklyEffort, int targetEffort)
    {
        // Arrange
        const byte expectedRecoveryEffortLevel = 1;
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBe(expectedRecoveryEffortLevel);
    }
}
