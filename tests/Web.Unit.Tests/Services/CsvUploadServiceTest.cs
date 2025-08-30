using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.Logging;
using Moq;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Assertions;
using RunSuggestion.Web.Constants;
using RunSuggestion.Web.Services;

namespace RunSuggestion.Web.Unit.Tests.Services;

[TestSubject(typeof(CsvUploadService))]
public class CsvUploadServiceTest
{
    private Mock<ILogger<CsvUploadService>> _mockLogger = new();
    private CsvUploadService _sut;

    public CsvUploadServiceTest()
    {
        _sut = new CsvUploadService(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "logger";
        ILogger<CsvUploadService> nullLogger = null!;

        // Act
        Func<CsvUploadService> withNullLoggerArgument = () => new CsvUploadService(nullLogger);

        // Assert
        ArgumentNullException ex = withNullLoggerArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("    ")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    [InlineData(null!)]
    public void Upload_WithEmptyCsvContent_ThrowsArgumentException(string csvContent)
    {
        // Arrange
        const string expectedParamName = "csvContent";
        const string expectedMessage = Errors.Upload.NoCsvContent;

        // Act
        Func<Task> withEmptyCsvContent = async () => await _sut.Upload(csvContent);

        // Assert
        ArgumentException ex = withEmptyCsvContent.ShouldThrow<ArgumentException>();
        ex.ParamName.ShouldBe(expectedParamName);
        ex.Message.ShouldContain(expectedMessage);
    }

    [Fact]
    public void Upload_WithNonEmptyCsvContent_LogsUploadStarted()
    {
        // Arrange
        const string csvContent = Any.String;
        const string expectedLog = Logs.Upload.Start;

        // Act
        _sut.Upload(csvContent);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedLog);
    }
}
