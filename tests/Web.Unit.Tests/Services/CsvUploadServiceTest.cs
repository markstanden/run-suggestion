using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RunSuggestion.Shared.Constants;
using RunSuggestion.TestHelpers;
using RunSuggestion.TestHelpers.Assertions;
using RunSuggestion.Web.Constants;
using RunSuggestion.Web.Services;
using static RunSuggestion.TestHelpers.Creators.HttpTestHelpers;

namespace RunSuggestion.Web.Unit.Tests.Services;

[TestSubject(typeof(CsvUploadService))]
public class CsvUploadServiceTest
{
    private readonly Mock<ILogger<CsvUploadService>> _mockLogger = new();

    private CsvUploadService CreateSut(int response = Any.Integer, HttpStatusCode status = HttpStatusCode.OK) =>
        CreateSutWithMockHttpMessageHandler(response, status).sut;

    private (CsvUploadService sut, Mock<HttpMessageHandler> mockHttpMessageHandler)
        CreateSutWithMockHttpMessageHandler(int response = Any.Integer, HttpStatusCode status = HttpStatusCode.OK)
    {
        Mock<HttpMessageHandler> mockHttpMessageHandler =
            CreateMockHttpMessageHandler(CreateResponse(response, status));
        HttpClient testHttpClient = new(mockHttpMessageHandler.Object) { BaseAddress = Any.Url };
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
    public async Task Upload_WithNonEmptyCsvContent_LogsUploadStarted()
    {
        // Arrange
        const string csvContent = Any.String;
        const string expectedLog = Logs.Upload.Start;
        CsvUploadService sut = CreateSut();

        // Act
        await sut.Upload(csvContent);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedLog);
    }

    [Fact]
    public async Task Upload_WithNonEmptyCsvContent_SendsToApiEndpoint()
    {
        // Arrange
        const string expectedApiEndpoint = Routes.UploadApiEndpoint;
        const string csvContent = Any.String;
        (CsvUploadService sut, Mock<HttpMessageHandler> mockHttpMessageHandler) = CreateSutWithMockHttpMessageHandler();

        // Act
        await sut.Upload(csvContent);

        // Assert
        mockHttpMessageHandler
            .Protected()
            .Verify(
                SendAsync,
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                                                  req.Method == HttpMethod.Post &&
                                                  req.RequestUri != null &&
                                                  req.RequestUri.ToString().Contains(expectedApiEndpoint)),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WithNonEmptyCsvContent_SendsExpectedCsvData()
    {
        // Arrange
        const string csvContent = Any.String;
        (CsvUploadService sut, Mock<HttpMessageHandler> mockHttpMessageHandler) = CreateSutWithMockHttpMessageHandler();

        // Act
        await sut.Upload(csvContent);

        // Assert
        mockHttpMessageHandler
            .Protected()
            .Verify(
                SendAsync,
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                                                  req.Content != null &&
                                                  req.Content.ReadAsStringAsync().Result == csvContent),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task Upload_WithInvalidCsvContent_ReturnsFalse()
    {
        // Arrange
        const string csvContent = Any.String;
        CsvUploadService sut = CreateSut(0, HttpStatusCode.BadRequest);

        // Act
        bool result = await sut.Upload(csvContent);

        // Assert
        result.ShouldBeFalse();
    }
}
