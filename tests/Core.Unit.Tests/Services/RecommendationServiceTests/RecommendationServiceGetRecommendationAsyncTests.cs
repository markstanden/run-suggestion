using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.Shared.Models.Users;
using RunSuggestion.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;
using static RunSuggestion.Shared.Constants.Runs;

namespace RunSuggestion.Core.Unit.Tests.Services.RecommendationServiceTests;

[TestSubject(typeof(RecommendationService))]
public class RecommendationServiceGetRecommendationAsyncTests
{
    private readonly DateTime _currentDate = new(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc);
    private readonly Mock<ILogger<RecommendationService>> _mockLogger = new();
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly RecommendationService _sut;

    public RecommendationServiceGetRecommendationAsyncTests()
    {
        _sut = new RecommendationService(_mockLogger.Object, _mockRepository.Object, _currentDate);
    }

    /// <summary>
    /// Helper method to help assert whether a <see cref="RunRecommendation"/>
    /// is the base run recommendation returned to beginners.
    /// </summary>
    /// <param name="runRec">The recommendation to compare against the base recommendation</param>
    /// <returns>true if the provided RunRecommendation is the base recommendation</returns>
    private static bool IsBaseRunRecommendation(RunRecommendation runRec) =>
        runRec.Distance == InsufficientHistory.RunDistanceMetres &&
        runRec.Effort == InsufficientHistory.RunEffort &&
        runRec.Duration == InsufficientHistory.RunDurationTimeSpan(InsufficientHistory.RunDistanceMetres);

    [Fact]
    public async Task GetRecommendationAsync_WhenCalled_LogsRecommendationRequest()
    {
        // Arrange
        string expectedMessage = RecommendationService.LogMessageCalled;

        // Act
        await _sut.GetRecommendationAsync(Any.String);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedMessage);
    }

    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public async Task GetRecommendationAsync_WithInvalidEntraId_ThrowsArgumentException(string invalidEntraId)
    {
        // Act
        Func<Task<RunRecommendation>> withInvalidEntraId =
            async () => await _sut.GetRecommendationAsync(invalidEntraId);

        // Assert
        Exception ex = await withInvalidEntraId.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("entraId");
    }

    [Theory]
    [MemberData(nameof(TestData.NullOrWhitespace), MemberType = typeof(TestData))]
    public async Task GetRecommendationAsync_WithInvalidEntraId_LogsInvalidIdFailure(string invalidEntraId)
    {
        // Arrange
        string expectedMessage = RecommendationService.LogMessageInvalidId;

        // Act
        Func<Task<RunRecommendation>> withInvalidEntraId =
            async () => await _sut.GetRecommendationAsync(invalidEntraId);
        await withInvalidEntraId.ShouldThrowAsync<ArgumentException>();

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Critical, expectedMessage);
    }

    [Theory]
    [InlineData(Any.LongAlphanumericString)]
    [InlineData(Any.ShortAlphanumericString)]
    public async Task GetRecommendationAsync_WithValidEntraId_CallsRepositoryWithProvidedId(string entraId)
    {
        // Arrange
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new UserData());

        // Act
        await _sut.GetRecommendationAsync(entraId);

        // Assert
        _mockRepository.Verify(x => x.GetUserDataByEntraIdAsync(entraId), Times.Once);
    }

    [Fact]
    public async Task GetRecommendationAsync_WithEmptyRunHistory_ReturnsBaseRunRecommendation()
    {
        // Arrange
        UserData userDataWithEmptyRunHistory = new()
        {
            RunHistory = []
        };
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userDataWithEmptyRunHistory);

        // Act
        RunRecommendation result = await _sut.GetRecommendationAsync(Any.LongAlphanumericString);

        // Assert
        result.ShouldNotBe(null);
        IsBaseRunRecommendation(result).ShouldBeTrue();
    }

    [Fact]
    public async Task GetRecommendationAsync_WithEmptyRunHistory_LogsBaseRecommendationResponse()
    {
        // Arrange
        string expectedMessage = RecommendationService.LogMessageInsufficientHistory;
        UserData userDataWithEmptyRunHistory = new()
        {
            RunHistory = []
        };
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userDataWithEmptyRunHistory);

        // Act
        await _sut.GetRecommendationAsync(Any.LongAlphanumericString);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedMessage);
    }

    [Fact]
    public async Task GetRecommendationAsync_WithRunHistory_ReturnsACustomRunRecommendation()
    {
        // Arrange
        UserData lowIntensityRunHistory = new()
        {
            RunHistory = RunBaseFakes.LowIntensityRunHistory(_currentDate)
        };
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(It.IsAny<string>()))
            .ReturnsAsync(lowIntensityRunHistory);

        // Act
        RunRecommendation result = await _sut.GetRecommendationAsync(Any.LongAlphanumericString);

        // Assert
        result.ShouldNotBe(null);
        IsBaseRunRecommendation(result).ShouldBeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(21)]
    [InlineData(28)]
    [InlineData(35)]
    public async Task GetRecommendationAsync_WithSomeFormOfRunHistory_ReturnsACustomRunRecommendation(int sampleSize)
    {
        // Arrange
        UserData lowIntensityRunHistory = new()
        {
            RunHistory = RunBaseFakes.LowIntensityRunHistory(_currentDate, sampleSize)
        };
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(It.IsAny<string>()))
            .ReturnsAsync(lowIntensityRunHistory);

        // Act
        RunRecommendation result = await _sut.GetRecommendationAsync(Any.LongAlphanumericString);

        // Assert
        result.ShouldNotBe(null);
        IsBaseRunRecommendation(result).ShouldBeFalse();
    }

    [Theory]
    [InlineData(5000)]
    [InlineData(10000)]
    [InlineData(15000)]
    public async Task GetRecommendationAsync_WithCurrentWeeksRunningAtLimit_ReturnsRestDayRecommendation(
        int baseDistanceMetres
    )
    {
        // Arrange
        int progressionPercent = RuleConfig.Default.SafeProgressionPercent;
        double progressionRatio = RecommendationService.CalculateProgressionRatio(progressionPercent);
        int progressionDistanceMetres = (int)Math.Round(baseDistanceMetres * progressionRatio);
        IEnumerable<RunEvent> currentWeekAtHistoricAverage =
        [
            RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(-1),
                                        distanceMetres: progressionDistanceMetres,
                                        effort: EffortLevel.Easy),
            RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(-2),
                                        distanceMetres: progressionDistanceMetres,
                                        effort: EffortLevel.Easy),
            RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(-3),
                                        distanceMetres: progressionDistanceMetres,
                                        effort: EffortLevel.Recovery),

            ..RunBaseFakes.CreateWeekOfRuns(baseDistanceMetres, _currentDate.AddDays(-7), 3),
            ..RunBaseFakes.CreateWeekOfRuns(baseDistanceMetres, _currentDate.AddDays(-14), 3),
            ..RunBaseFakes.CreateWeekOfRuns(baseDistanceMetres, _currentDate.AddDays(-21), 3),
            ..RunBaseFakes.CreateWeekOfRuns(baseDistanceMetres, _currentDate.AddDays(-28), 3)
        ];

        UserData userData = new()
        {
            RunHistory = currentWeekAtHistoricAverage
        };
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userData);

        // Act
        RunRecommendation result = await _sut.GetRecommendationAsync(Any.LongAlphanumericString);

        // Assert
        result.ShouldNotBe(null);
        result.Distance.ShouldBe(0);
        result.Effort.ShouldBe(EffortLevel.Rest);
        result.Duration.ShouldBe(RestDuration);
    }

    [Theory]
    [InlineData(5000, 1)]
    [InlineData(10000, 1)]
    [InlineData(15000, 1)]
    [InlineData(5000, 1000)]
    [InlineData(10000, 1000)]
    [InlineData(15000, 1000)]
    [InlineData(5000, 10000)]
    [InlineData(10000, 10000)]
    [InlineData(15000, 10000)]
    public async Task GetRecommendationAsync_WithCurrentWeeksRunningOverLimit_ReturnsRestDayRecommendation(
        int baseDistanceMetres, int overtrainingMetres
    )
    {
        // Arrange
        int progressionPercent = RuleConfig.Default.SafeProgressionPercent;
        double progressionRatio = RecommendationService.CalculateProgressionRatio(progressionPercent);
        int progressionDistanceMetres = (int)Math.Round(baseDistanceMetres * progressionRatio);
        IEnumerable<RunEvent> currentWeekAtHistoricAverage =
        [
            RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(-1),
                                        distanceMetres: progressionDistanceMetres + overtrainingMetres,
                                        effort: EffortLevel.Easy),
            RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(-2),
                                        distanceMetres: progressionDistanceMetres,
                                        effort: EffortLevel.Easy),
            RunBaseFakes.CreateRunEvent(dateTime: _currentDate.AddDays(-3),
                                        distanceMetres: progressionDistanceMetres,
                                        effort: EffortLevel.Recovery),

            ..RunBaseFakes.CreateWeekOfRuns(baseDistanceMetres, _currentDate.AddDays(-7), 3),
            ..RunBaseFakes.CreateWeekOfRuns(baseDistanceMetres, _currentDate.AddDays(-14), 3),
            ..RunBaseFakes.CreateWeekOfRuns(baseDistanceMetres, _currentDate.AddDays(-21), 3),
            ..RunBaseFakes.CreateWeekOfRuns(baseDistanceMetres, _currentDate.AddDays(-28), 3)
        ];

        UserData userData = new()
        {
            RunHistory = currentWeekAtHistoricAverage
        };
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(It.IsAny<string>()))
            .ReturnsAsync(userData);

        // Act
        RunRecommendation result = await _sut.GetRecommendationAsync(Any.LongAlphanumericString);

        // Assert
        result.ShouldNotBe(null);
        result.Distance.ShouldBe(0);
        result.Effort.ShouldBe(EffortLevel.Rest);
        result.Duration.ShouldBe(RestDuration);
    }
}
