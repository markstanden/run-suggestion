using System.Runtime.InteropServices.JavaScript;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;
using RunSuggestion.Web.Constants;
using RunSuggestion.Web.Services;
using static RunSuggestion.TestHelpers.Creators.HttpTestHelpers;

namespace RunSuggestion.Web.Unit.Tests.Services;

[TestSubject(typeof(CsvUploadService))]
public class CsvUploadServiceTest
{
    private readonly Mock<ILogger<CsvUploadService>> _mockLogger = new();

    private CsvUploadService CreateSut(bool response = true, string baseAddress = Any.UrlString) =>
        CreateSutWithMockHttpMessageHandler(response, baseAddress).sut;

    private (CsvUploadService sut, Mock<HttpMessageHandler> mockHttpMessageHandler)
        CreateSutWithMockHttpMessageHandler(bool response = true, string baseAddress = Any.UrlString)
    {
        Mock<HttpMessageHandler> mockHttpMessageHandler = CreateMockHttpMessageHandler(CreateResponse(response));
        HttpClient testHttpClient = new(mockHttpMessageHandler.Object) { BaseAddress = new Uri(baseAddress) };
        CsvUploadService sut = new(_mockLogger.Object, testHttpClient);
        return (sut, mockHttpMessageHandler);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "logger";
        ILogger<CsvUploadService> nullLogger = null!;
        HttpClient anyClient = new();

        // Act
        Func<CsvUploadService> withNullLoggerArgument = () => new CsvUploadService(nullLogger, anyClient);

        // Assert
        ArgumentNullException ex = withNullLoggerArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "httpClient";
        HttpClient nullClient = null!;

        // Act
        Func<CsvUploadService> withNullLoggerArgument = () => new CsvUploadService(_mockLogger.Object, nullClient);

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
        CsvUploadService sut = CreateSut();

        // Act
        Func<Task> withEmptyCsvContent = async () => await sut.Upload(csvContent);

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
        CsvUploadService sut = CreateSut();

        // Act
        sut.Upload(csvContent);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedLog);
    }

    [Fact]
    public void Upload_WithNonEmptyCsvContent_SendsToApiEndpoint()
    {
        // Arrange
        const string csvContent = Any.String;
        (CsvUploadService sut, Mock<HttpMessageHandler> mockHttpMessageHandler) = CreateSutWithMockHttpMessageHandler();

        // Act
        sut.Upload(csvContent);

        // Assert
        mockHttpMessageHandler
            .Protected()
            .Verify(
                SendAsync,
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }
}
