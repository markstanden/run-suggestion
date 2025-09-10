using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.Shared.Models.Users;

namespace RunSuggestion.Core.Unit.Tests.Services;

[TestSubject(typeof(RecommendationService))]
public class RecommendationServiceTests
{
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly Mock<ILogger<RecommendationService>> _mockLogger = new();
    private readonly RecommendationService _sut;

    public RecommendationServiceTests()
    {
        _sut = new RecommendationService(_mockLogger.Object, _mockRepository.Object);
    }

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
}
