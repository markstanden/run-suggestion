using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;

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
    public void CalculateEffort_WithEmptyRunHistory_ReturnsDefaultEffort()
    {
        // Arrange
        TimeSpan expectedTimeSpan = Runs.InsufficientHistory.RunDurationTimeSpan;
        IEnumerable<RunEvent> runEvents = [];

        // Act
        TimeSpan result = _sut.CalculateDuration(runEvents);

        // Assert
        result.ShouldBe(expectedTimeSpan);
    }
}
