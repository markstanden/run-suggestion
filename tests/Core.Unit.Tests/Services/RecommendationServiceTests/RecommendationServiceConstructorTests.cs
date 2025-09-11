using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;

namespace RunSuggestion.Core.Unit.Tests.Services.RecommendationServiceTests;

[TestSubject(typeof(RecommendationService))]
public class RecommendationServiceConstructorTests
{
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly Mock<ILogger<RecommendationService>> _mockLogger = new();

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
}
