using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Tests.TestHelpers;
using RunSuggestion.Core.Validators;

namespace RunSuggestion.Core.Tests.Validators;

public class RunEventValidatorTests
{
    private RunEventValidator _sut;
    private DateTime _currentDate = new(2025, 7, 1);

    public RunEventValidatorTests()
    {
        _sut = new RunEventValidator(_currentDate);
    }

    [Fact]
    public void Validate_WithNullEvents_ThrowsArgumentException()
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = null!; ;

        // Act
        var withNullEvents = () => _sut.Validate(runEvents);

        // Assert
        Exception ex = withNullEvents.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("RunEvents");
    }

    [Fact]
    public void Validate_WithNullEventDate_ReturnsDateError()
    {
        // Arrange
        DateTime tomorrow = _currentDate.AddDays(1);
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent(
            dateTime: tomorrow)];

        // Act
        IEnumerable<string> errors = _sut.Validate(runEvents);

        // Assert
        errors.ShouldNotBeEmpty();
        errors.First().ShouldContain("Invalid");
        errors.First().ShouldContain("Date");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_WithInvalidDistance_ReturnsDistanceError(int invalidDistance)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent(
            dateTime: _currentDate,
            distanceMetres: invalidDistance)];

        // Act
        IEnumerable<string> errors = _sut.Validate(runEvents);

        // Assert
        errors.ShouldNotBeEmpty();
        errors.First().ShouldContain("Invalid");
        errors.First().ShouldContain("Distance");
    }

    [Theory]
    [InlineData(11)]
    [InlineData(byte.MaxValue)]
    public void Validate_WithInvalidEffort_ReturnsEffortError(byte invalidEffort)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent(
            dateTime: _currentDate,
            effort: invalidEffort)];

        // Act
        IEnumerable<string> errors = _sut.Validate(runEvents);

        // Assert
        errors.ShouldNotBeEmpty();
        errors.First().ShouldContain("Invalid");
        errors.First().ShouldContain("Effort");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_WithInvalidDuration_ReturnsDurationError(int invalidDuration)
    {
        // Arrange
        TimeSpan invalidDurationTimeSpan = TimeSpan.FromSeconds(invalidDuration);
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent(
            dateTime: _currentDate,
            duration: invalidDurationTimeSpan)];

        // Act
        IEnumerable<string> errors = _sut.Validate(runEvents);

        // Assert
        errors.ShouldNotBeEmpty();
        errors.First().ShouldContain("Invalid");
        errors.First().ShouldContain("Duration");
    }
}