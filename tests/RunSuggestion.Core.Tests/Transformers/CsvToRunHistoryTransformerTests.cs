using RunSuggestion.Core.Models;
using RunSuggestion.Core.Models.DataSources.TrainingPeaks;
using RunSuggestion.Core.Tests.TestHelpers;
using RunSuggestion.Core.Transformers;

namespace RunSuggestion.Core.Tests.Transformers;

public class CsvToRunHistoryTransformerTests
{
    private readonly CsvToRunHistoryTransformer _sut = new();

    [Fact]
    public void ConvertToRunHistory_SingleValidRunRow_ReturnsOneRunEvent()
    {
        // Arrange
        string csv = new TrainingPeaksCsvBuilder()
            .AddRunningRow()
            .Build();

        // Act
        IEnumerable<RunEvent> result = _sut.Transform(csv);

        // Assert
        RunEvent runEvent = result.ShouldHaveSingleItem();
    }

    [Theory]
    [InlineData(2024, 12, 31)]
    [InlineData(2025, 1, 1)]
    public void ConvertToRunHistory_WhenPassedCorrectDateFormat_ParsesDateCorrectly(int year, int month, int day)
    {
        // Arrange
        DateTime expectedDate = new(year, month, day);
        string dateString = expectedDate.ToString(TrainingPeaksActivity.WORKOUT_DAY_DATETIME_FORMAT);
        string csv = new TrainingPeaksCsvBuilder()
            .AddRunningRow(dateString)
            .Build();

        // Act
        IEnumerable<RunEvent> result = _sut.Transform(csv);

        // Assert
        RunEvent runEvent = result.ShouldHaveSingleItem();
        runEvent.Date.ShouldBe(expectedDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(1000)]
    [InlineData(10000)]
    [InlineData(100000)]
    public void ConvertToRunHistory_WhenPassedValidDistance_ParsesDistanceCorrectly(int distance)
    {
        // Arrange
        string csv = new TrainingPeaksCsvBuilder()
            .AddRunningRow(distanceInMeters: distance)
            .Build();

        // Act
        IEnumerable<RunEvent> result = _sut.Transform(csv);

        // Assert
        RunEvent runEvent = result.ShouldHaveSingleItem();
        runEvent.Distance.ShouldBe(distance);
    }

    [Theory]
    [InlineData("Running", "Cardio", "Yoga", "Breathwork", 1)]
    [InlineData("Running", "Running", "Yoga", "Breathwork", 2)]
    [InlineData("Running", "Running", "Running", "Breathwork", 3)]
    [InlineData("Running", "Running", "Running", "Running", 4)]
    [InlineData("Cardio", "Running", "Yoga", "Breathwork", 1)]
    [InlineData("Cardio", "Yoga", "Running", "Breathwork", 1)]
    [InlineData("Cardio", "Yoga", "Breathwork", "Running", 1)]
    public void ConvertToRunHistory_WhenPassedMultipleValidRowsWithMixedActivityTypes_ReturnsOnlyRunEvents(
        string activity1, string activity2, string activity3, string activity4, int expectedCount)
    {
        // Arrange
        string csv = new TrainingPeaksCsvBuilder()
            .AddRow(activity1)
            .AddRow(activity2)
            .AddRow(activity3)
            .AddRow(activity4)
            .Build();

        // Act
        IEnumerable<RunEvent> results = _sut.Transform(csv);

        // Assert
        results.Count().ShouldBe(expectedCount);
    }

    [Fact]
    public void Transform_WithNoActivityDataWithinCSV_ReturnsAnEmptyCollection()
    {
        // Arrange
        string csv = new TrainingPeaksCsvBuilder().Build();

        // Act
        IEnumerable<RunEvent> results = _sut.Transform(csv);

        // Assert
        results.ShouldBeEmpty();
    }

    [Fact]
    public void Transform_WithAnEmptyCSV_ReturnsAnEmptyCollection()
    {
        // Arrange
        string csv = string.Empty;

        // Act
        IEnumerable<RunEvent> results = _sut.Transform(csv);

        // Assert
        results.ShouldBeEmpty();
    }

}
