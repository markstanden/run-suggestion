using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;
using RunSuggestion.Core.Services;
using RunSuggestion.Core.Unit.Tests.TestHelpers;

namespace RunSuggestion.Core.Unit.Tests.Services;

public class TrainingPeaksHistoryServiceTests
{
    private readonly Mock<IRunHistoryTransformer> _mockTransformer = new();
    private readonly Mock<IUserRepository> _mockRepository = new();
    private readonly TrainingPeaksHistoryService _sut;

    public TrainingPeaksHistoryServiceTests()
    {
        _sut = new TrainingPeaksHistoryService(_mockRepository.Object, _mockTransformer.Object);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddRunHistory_WithInvalidEntraId_ThrowsArgumentException(string invalidEntraId)
    {
        // Arrange
        string validCsv = new TrainingPeaksCsvBuilder().Build();

        // Act
        var withInvalidEntraId = async () => await _sut.AddRunHistory(invalidEntraId, validCsv);

        // Assert
        Exception ex = await withInvalidEntraId.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("entraId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddRunHistory_WithInvalidCsv_ThrowsArgumentException(string? invalidCsv)
    {
        // Arrange
        string validEntraId = Fakes.CreateEntraId();

        // Act
        var withInvalidCsv = async () => await _sut.AddRunHistory(validEntraId, invalidCsv!);

        // Assert
        Exception ex = await withInvalidCsv.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("csv");
    }
    
    [Fact]
    public async Task AddRunHistory_WhenPassedValidCsv_CallsTransformerOnceWithPassedCsv()
    {
        // Arrange
        string validEntraId = Fakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        
        // Act
        await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        _mockTransformer.Verify(x => x.Transform(validCsv), Times.Once);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_WithExistingUser_CallsRepositoryWithCorrectUserId(int expectedUserId)
    {
        // Arrange
        string entraId = Fakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        UserData userData = new() { UserId = expectedUserId, EntraId = entraId };
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(entraId))
            .ReturnsAsync(userData);

        // Act
        await _sut.AddRunHistory(entraId, validCsv);

        // Assert
        _mockRepository.Verify(x => x.AddRunEventsAsync(expectedUserId, It.IsAny<IEnumerable<RunEvent>>()), Times.Once);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_WithNewUser_CallsRepositoryWithCorrectUserId(int expectedUserId)
    {
        // Arrange
        string entraId = Fakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        // Call to get userdata returns null for a not found user
        _mockRepository.Setup(x => x.GetUserDataByEntraIdAsync(entraId))
            .ReturnsAsync(null as UserData);
        // Call to create a new user returns our expectedUserId
        _mockRepository.Setup(x => x.CreateUserAsync(entraId))
            .ReturnsAsync(expectedUserId);

        // Act
        await _sut.AddRunHistory(entraId, validCsv);

        // Assert
        _mockRepository.Verify(x => x.AddRunEventsAsync(expectedUserId, It.IsAny<IEnumerable<RunEvent>>()), Times.Once);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_WhenTransformerReturnsValidRunEvents_CallsRepositoryWithCorrectRunEvents(int expectedEventCount)
    {
        // Arrange
        string validEntraId = Fakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        var expectedRunEvents = Fakes.CreateRunEvents(expectedEventCount).ToList();
        _mockTransformer.Setup(x => x.Transform(It.IsAny<string>()))
            .Returns(expectedRunEvents);
        _mockRepository.Setup(x => x.AddRunEventsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<RunEvent>>()))
            .ReturnsAsync(expectedEventCount);

        // Act
        await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        _mockRepository.Verify(x => x.AddRunEventsAsync(It.IsAny<int>(), expectedRunEvents), Times.Once);
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(10, 0)]
    [InlineData(10, 1)]
    [InlineData(10, 10)]
    [InlineData(100, 0)]
    [InlineData(100, 1)]
    [InlineData(100, 99)]
    [InlineData(100, 100)]
    public async Task AddRunHistory_WhenRepositoryMethodReturns_ReturnsActualAffectedLines(int expectedEventCount, int actualAffectedLines)
    {
        // Arrange
        string validEntraId = Fakes.CreateEntraId();
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        var expectedRunEvents = Fakes.CreateRunEvents(expectedEventCount);
        _mockTransformer.Setup(x => x.Transform(It.IsAny<string>()))
            .Returns(expectedRunEvents);
        _mockRepository.Setup(x => x.AddRunEventsAsync(It.IsAny<int>(), It.IsAny<IEnumerable<RunEvent>>())).ReturnsAsync(actualAffectedLines);

        // Act
        int result = await _sut.AddRunHistory(validEntraId, validCsv);

        // Assert
        result.ShouldBe(actualAffectedLines);
    }
}