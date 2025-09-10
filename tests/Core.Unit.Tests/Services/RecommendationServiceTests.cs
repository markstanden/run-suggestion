using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.Shared.Models.Users;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Core.Unit.Tests.Services;

[TestSubject(typeof(RecommendationService))]
public class RecommendationServiceTests
{
    private readonly DateTime _currentDate = new(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc);
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly Mock<ILogger<RecommendationService>> _mockLogger = new();
    private readonly RecommendationService _sut;

    public RecommendationServiceTests()
    {
        _sut = new RecommendationService(_mockLogger.Object, _mockRepository.Object, _currentDate);
    }

    /// <summary>
    /// Helper method to help assert whether a <see cref="RunRecommendation"/>
    /// is the base run recommendation returned to beginners.
    /// </summary>
    /// <param name="runRecommendation">The recommendation to compare against the base recommendation</param>
    /// <returns>true if the provided RunRecommendation is the base recommendation</returns>
    private static bool IsBaseRunRecommendation(RunRecommendation runRecommendation) =>
        runRecommendation.Distance == Runs.RunDistanceBaseMetres &&
        runRecommendation.Effort == Runs.RunEffortBase &&
        runRecommendation.Duration == Runs.RunDistanceBaseDurationTimeSpan;

    [Fact]
    public void Constructor_WithNullLoggerArgument_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "logger";
        ILogger<RecommendationService> nullLoggerArgument = null!;

        // Act
        Func<RecommendationService> withNullLoggerArgument = () =>
            new RecommendationService(nullLoggerArgument, _mockRepository.Object);

        // Assert
        ArgumentNullException ex = withNullLoggerArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    [Fact]
    public void Constructor_WithNullRepositoryArgument_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "userRepository";
        IUserRepository nullRepositoryArgument = null!;

        // Act
        Func<RecommendationService> withNullRepositoryArgument = () =>
            new RecommendationService(_mockLogger.Object, nullRepositoryArgument);

        // Assert
        ArgumentNullException ex = withNullRepositoryArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
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
    [InlineData(1500, 5, 525)]
    [InlineData(3000, 5, 1050)]
    [InlineData(4500, 5, 1575)]
    [InlineData(3000, 10, 1100)]
    [InlineData(3000, 20, 1200)]
    public void CalculateDistance_With3RunsWeeklyAndProvidedProgressionPercent_ProvidesExpectedDistance(
        int weeklyAverage,
        int percentageProgression, int expectedDistance)
    {
        // Arrange
        int accuracyMetres = 10;
        int runDistance = weeklyAverage / 3;
        IEnumerable<RunEvent> runEvents =
        [
            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-3)),
            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-5)),

            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-8)),
            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-10)),
            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-12)),

            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-15)),
            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-17)),
            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-19)),

            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-22)),
            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-24)),
            RunBaseFakes.CreateRunEvent(distanceMetres: runDistance, dateTime: _currentDate.AddDays(-26))
        ];

        // Act
        int result = _sut.CalculateDistance(runEvents, percentageProgression);

        // Assert
        result.ShouldBeInRange(expectedDistance - accuracyMetres, expectedDistance + accuracyMetres);
    }
}
