using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Tests.TestHelpers;
using RunSuggestion.Core.Validators;

namespace RunSuggestion.Core.Tests.Validators;

public class RunEventValidatorTests
{
    private RunEventValidator _sut;
    private DateTime _currentDate = DateTime.Parse("2025-07-01");
    
    public RunEventValidatorTests()
    {
        _sut = new RunEventValidator(_currentDate);
    }
    
    [Fact]
    public void Validate_WithNullEventDate_ReturnsDateError()
    {
        // Arrange
        DateTime tomorrow = _currentDate.AddDays(1);
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent(dateTime: tomorrow)];

        // Act
        string[] errors = _sut.Validate(runEvents);

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
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent(distanceMetres: invalidDistance)];

        // Act
        string[] errors = _sut.Validate(runEvents);

        // Assert
        errors.ShouldNotBeEmpty();
        errors.First().ShouldContain("Invalid");
        errors.First().ShouldContain("Distance");
    }
    
    [Theory]
    [InlineData(-1)]
    [InlineData(byte.MinValue)]
    [InlineData(11)]
    [InlineData(byte.MaxValue)]
    public void Validate_WithInvalidEffort_ReturnsEffortError(byte invalidEffort)
    {
        // Arrange
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent(effort: invalidEffort)];

        // Act
        string[] errors = _sut.Validate(runEvents);

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
        IEnumerable<RunEvent> runEvents = [Fakes.CreateRunEvent(duration: invalidDurationTimeSpan)];

        // Act
        string[] errors = _sut.Validate(runEvents);

        // Assert
        errors.ShouldNotBeEmpty();
        errors.First().ShouldContain("Invalid");
        errors.First().ShouldContain("Duration");
    }
}