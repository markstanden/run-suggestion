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
    public void AddRunHistory_WhenTransformerReturnsValidRunEvents_ReturnsExpectedCount(int expectedEventCount)
    {
        // Arrange
        int anyUserId = 1;
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        var expectedRunEvents = Fakes.CreateRunEvents(expectedEventCount);
        _mockTransformer.Setup(x => x.Transform(It.IsAny<string>()))
            .Returns(expectedRunEvents);

        // Act
        int result = _sut.AddRunHistory(anyUserId, validCsv);

        // Assert
        result.ShouldBe(expectedEventCount);
    }
    
    [Fact]
    public void AddRunHistory_WhenPassedValidCsv_CallsTransformerOnceWithCsv()
    {
        // Arrange
        int anyUserId = 1;
        string validCsv = new TrainingPeaksCsvBuilder().Build();
        
        // Act
        _sut.AddRunHistory(anyUserId, validCsv);

        // Assert
        _mockTransformer.Verify(x => x.Transform(validCsv), Times.Once);
    }
}