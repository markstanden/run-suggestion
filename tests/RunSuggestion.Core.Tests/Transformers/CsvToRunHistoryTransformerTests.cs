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
        const string CsvHeader = "\"Title\",\"WorkoutType\",\"WorkoutDay\",\"DistanceInMeters\",\"TimeTotalInHours\",\"HeartRateAverage\",\"HeartRateMax\",\"Rpe\",\"Feeling\"";
        const string CsvRow = "\"Running\",\"Run\",\"2024-12-31\",\"12345.6789\",\"0.5\",\"150\",\"200\",\"3\",\"5\"";
        const string Csv = $"{CsvHeader}\n{CsvRow}";

        // Act
        IEnumerable<RunEvent> result = _sut.Transform(Csv);

        // Assert
        RunEvent runEvent = result.ShouldHaveSingleItem();
        runEvent.Date.ShouldBe(new DateTime(2024, 12, 31));
        runEvent.Distance.ShouldBe(12346);
        runEvent.Duration.ShouldBe(TimeSpan.FromMinutes(30));
        runEvent.Effort.ShouldBe((byte)3);
    }
}
