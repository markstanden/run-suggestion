using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.Shared.Models.Users;
using RunSuggestion.TestHelpers.Creators;

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
    /// <param name="runRecommendation">The recommendation to compare against the base recommendation</param>
    /// <returns>true if the provided RunRecommendation is the base recommendation</returns>
    private static bool IsBaseRunRecommendation(RunRecommendation runRecommendation) =>
        runRecommendation.Distance == Runs.RunDistanceBaseMetres &&
        runRecommendation.Effort == Runs.EffortLevel.Easy &&
        runRecommendation.Duration == Runs.RunDistanceBaseDurationTimeSpan;

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
}
