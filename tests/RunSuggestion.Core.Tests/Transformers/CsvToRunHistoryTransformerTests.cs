using RunSuggestion.Core.Models;
using RunSuggestion.Core.Transformers;

namespace RunSuggestion.Core.Tests.Transformers;

public class CsvToRunHistoryTransformerTests
{
    private readonly CsvToRunHistoryTransformer _sut = new();

    [Fact]
    public void ConvertToRunHistory_SingleValidRow_ReturnsOneRunEvent()
    {
        // Arrange
        const string CsvHeader = "\"WorkoutDay\",\"DistanceInMeters\",\"TimeTotalInHours\",\"Rpe\"";
        const string CsvRow = "\"2025-01-01\",\"6073.669921875\",\"0.551279187202454\",\"\"";
        const string Csv = $"{CsvHeader}\n{CsvRow}";

        // Act
        IEnumerable<RunEvent> result = _sut.Transform(Csv);

        // Assert
        RunEvent runEvent = result.ShouldHaveSingleItem();
        runEvent.Distance.ShouldBe(6.07f, 0.01f);
        runEvent.Duration.ShouldBe(TimeSpan.FromMinutes(33));
        runEvent.Effort.ShouldBe((byte)0);
    }
}
