using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Services;
using RunSuggestion.Core.Unit.Tests.TestHelpers;

namespace RunSuggestion.Core.Unit.Tests.Services;

public class TrainingPeaksHistoryServiceTests
{
    private readonly Mock<IRunHistoryTransformer> _mockTransformer = new();
    private readonly TrainingPeaksHistoryService _sut;

    public TrainingPeaksHistoryServiceTests()
    {
        _sut = new TrainingPeaksHistoryService(_mockTransformer.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task AddRunHistory_WhenTransformerReturnsValidRunEvents_ReturnsExpectedCount(int expectedEventCount)
    {
        // Arrange
        int anyUserId = 1;
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        var expectedRunEvents = Fakes.CreateRunEvents(expectedEventCount);
        _mockTransformer.Setup(x => x.Transform(It.IsAny<string>()))
            .Returns(expectedRunEvents);

        // Act
        int result = await _sut.AddRunHistory(anyUserId, validCsv);

        // Assert
        result.ShouldBe(expectedEventCount);
    }
    
    [Fact]
    public async Task AddRunHistory_WhenPassedValidCsv_CallsTransformerOnceWithCsv()
    {
        // Arrange
        int anyUserId = 1;
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        
        // Act
        await _sut.AddRunHistory(anyUserId, validCsv);

        // Assert
        _mockTransformer.Verify(x => x.Transform(validCsv), Times.Once);
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public async Task AddRunHistory_WithInvalidUserId_ThrowsArgumentException(int invalidUserId)
    {
        // Arrange
        string validCsv = new TrainingPeaksCsvBuilder().Build();

        // Act
        var withInvalidUserId = async () => await _sut.AddRunHistory(invalidUserId, validCsv);

        // Assert
        Exception ex = await withInvalidUserId.ShouldThrowAsync<ArgumentOutOfRangeException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("userId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task AddRunHistory_WithInvalidCsv_ThrowsArgumentException(string? invalidCsv)
    {
        // Arrange
        int validUserId = 1;

        // Act
        var withInvalidCsv = async () => await _sut.AddRunHistory(validUserId, invalidCsv!);

        // Assert
        Exception ex = await withInvalidCsv.ShouldThrowAsync<ArgumentException>();
        ex.Message.ShouldContain("Invalid");
        ex.Message.ShouldContain("csv");
    }
}