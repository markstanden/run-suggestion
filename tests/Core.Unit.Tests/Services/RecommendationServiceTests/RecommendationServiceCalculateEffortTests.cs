using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.TestHelpers.Creators;
using static RunSuggestion.Shared.Constants.Runs.EffortLevel;

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
    [InlineData(new[] { Recovery, Recovery, Recovery, Recovery }, 20)]
    [InlineData(new[] { Recovery, Recovery, Recovery }, 25)]
    public void CalculateEffort_WithAnyRunHistory_ReturnsMaxStrongEffort(byte[] currentWeeklyEffort,
        int targetEffort)
    {
        // Arrange
        const byte expectedStrongEffortLevel = Strong;
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBeLessThanOrEqualTo(expectedStrongEffortLevel);
    }

    [Theory]
    [InlineData(new[] { Recovery, Recovery, Recovery, Recovery }, 20)]
    [InlineData(new[] { Recovery, Recovery, Recovery }, 25)]
    [InlineData(new[] { Easy, Recovery, Easy, Recovery }, 20)]
    [InlineData(new[] { Easy, Easy, Easy, Easy }, 20)]
    [InlineData(new[] { Easy, Easy, Easy }, 25)]
    public void CalculateEffort_WithMostRunsAtLowEffort_ReturnsStrongEffort(byte[] currentWeeklyEffort,
        int targetEffort)
    {
        // Arrange
        const byte expectedStrongEffortLevel = Strong;
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBe(expectedStrongEffortLevel);
    }

    [Theory]
    [InlineData(new[] { Strong, Recovery, Recovery, Recovery }, 20)]
    [InlineData(new[] { Strong, Recovery, Easy, Recovery }, 20)]
    [InlineData(new[] { Strong, Easy, Easy, Easy }, 20)]
    [InlineData(new[] { Strong, Recovery, Recovery }, 25)]
    [InlineData(new[] { Strong, Easy, Easy }, 25)]
    public void CalculateEffort_WithRunsAlreadyMeetingTargetEffortPercentage_ReturnsEasyEffort(
        byte[] currentWeeklyEffort, int targetEffort)
    {
        // Arrange
        const byte expectedEasyEffortLevel = Easy;
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBe(expectedEasyEffortLevel);
    }

    [Theory]
    [InlineData(new[] { Strong, Recovery, Medium, Recovery }, 20)]
    [InlineData(new[] { Strong, Easy, Medium, Recovery }, 20)]
    [InlineData(new[] { Strong, Recovery, Medium, Easy }, 20)]
    [InlineData(new[] { Strong, Easy, Medium, Easy }, 20)]
    [InlineData(new[] { Strong, Recovery, Medium }, 25)]
    [InlineData(new[] { Strong, Easy, Medium }, 25)]
    public void CalculateEffort_WithRunsAlreadyExceedingTargetEffortPercentage_ReturnsRecoveryEffort(
        byte[] currentWeeklyEffort, int targetEffort)
    {
        // Arrange
        const byte expectedRecoveryEffortLevel = Recovery;
        IEnumerable<RunEvent> runEvents = currentWeeklyEffort
            .Select((eff, index) =>
                        RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(6 - index), effort: eff));

        // Act  
        byte result = _sut.CalculateEffort(runEvents, targetEffort);

        // Assert
        result.ShouldBe(expectedRecoveryEffortLevel);
    }
}
