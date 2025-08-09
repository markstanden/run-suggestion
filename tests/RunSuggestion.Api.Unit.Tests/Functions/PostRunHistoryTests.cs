using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RunSuggestion.Api.Dto;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.DataSources.TrainingPeaks;
using RunSuggestion.Core.Unit.Tests.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Api.Unit.Tests.Functions;

public class PostRunHistoryTests
{
    private readonly Mock<ILogger<PostRunHistory>> _mockLogger = new();
    private readonly Mock<IAuthenticator> _mockAuthenticator = new();
    private readonly Mock<IRunHistoryAdder> _mockHistoryAdder = new();

    private readonly PostRunHistory _sut;

    public PostRunHistoryTests()
    {
        _sut = new PostRunHistory(_mockLogger.Object, _mockAuthenticator.Object, _mockHistoryAdder.Object);
    }

    [Fact]
    public async Task Run_WhenCalled_LogsThatRunHistoryUploadProcessHasStarted()
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, "Run history upload started");
    }

    [Fact]
    public async Task Run_WithAuthHeaderNotSet_CallsAuthenticatorWithEmptyString()
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();

        // Act
        await _sut.Run(request);

        // Assert
        _mockAuthenticator.Verify(x => x.Authenticate(string.Empty), Times.Once);
    }

    [Theory]
    [InlineData("Bearer fake-token-12345")]
    [InlineData("Bearer fake-token-with-different-format")]
    [InlineData("Bearer 00fake00token00abcdefghijklmnopqrstuvwxyz0123456789")]
    public async Task Run_WhenCalledWithAuthHeaderSet_CallsAuthenticatorWithHeaderValue(string authToken)
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        request.Headers["Authorization"] = authToken;

        // Act
        await _sut.Run(request);

        // Assert
        _mockAuthenticator.Verify(x => x.Authenticate(authToken), Times.Once);
    }

    [Theory]
    [InlineData("fake-entra-id-12345")]
    [InlineData("fake-entra-id-with-different-format")]
    [InlineData("00fake00entra00id00abcdefghijklmnopqrstuvwxyz0123456789")]
    public async Task Run_WhenAuthenticationSucceeds_CallsHistoryAdderWithEntraId(string entraId)
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(entraId);

        // Act
        await _sut.Run(request);

        // Assert
        _mockHistoryAdder.Verify(x => x.AddRunHistory(entraId, It.IsAny<string>()), Times.Once);
    }

    [Theory]
    [InlineData("fake-entra-id-12345")]
    [InlineData("fake-entra-id-with-different-format")]
    [InlineData("00fake00entra00id00abcdefghijklmnopqrstuvwxyz0123456789")]
    public async Task Run_WhenAuthenticationSucceeds_LogsLastFiveOfEntraId(string entraId)
    {
        // Arrange
        string expectedLastFive = entraId.Substring(entraId.Length - 5);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(entraId);

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information,
                                         "Successfully Authenticated user",
                                         expectedLastFive);
    }

    [Fact]
    public async Task Run_WhenAuthenticationFails_Returns401UnauthorizedResponse()
    {
        // Arrange
        int expectedStatusCode = StatusCodes.Status401Unauthorized;
        string? nullEntraId = null;
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(nullEntraId);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        UnauthorizedResult unauthorizedResult = result.ShouldBeOfType<UnauthorizedResult>();
        unauthorizedResult.StatusCode.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public async Task Run_WhenAuthenticationFails_DoesNotCallHistoryAdder()
    {
        // Arrange
        string? nullEntraId = null;
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(nullEntraId);

        // Act
        await _sut.Run(request);

        // Assert
        _mockHistoryAdder.Verify(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Run_WhenAuthenticationFails_LogsFailedRequestAsWarning()
    {
        // Arrange
        string? nullEntraId = null;
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(nullEntraId);

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Warning, "Failed to authenticate user.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task Run_WhenAuthenticationSucceedsAndCsvPresent_CsvIsPassedIntoHistoryAdder(int csvRows)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        IEnumerable<TrainingPeaksActivity> activities = TrainingPeaksActivityFakes.CreateRandomRuns(csvRows);
        string csv = TrainingPeaksCsvBuilder.CsvFromActivities(activities);
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, csv);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());

        // Act
        await _sut.Run(request);

        // Assert
        _mockHistoryAdder.Verify(x => x.AddRunHistory(It.IsAny<string>(), csv), Times.Once);
    }

    [Fact]
    public async Task Run_WhenAuthenticationSucceedsAndValidCsvPresent_Returns200OkResult()
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task Run_WhenAuthenticationSucceedsAndValidCsvPresent_ReturnsSuccess()
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(1);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        UploadResponse response = okResult.Value.ShouldBeOfType<UploadResponse>();
        response.Message.ShouldContain("Success");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task Run_WhenAuthenticationSucceedsAndValidCsvPresent_ExpectedRowsReturned(
        int expectedAffectedRows)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedAffectedRows);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        UploadResponse response = okResult.Value.ShouldBeOfType<UploadResponse>();
        response.RowsAdded.ShouldBe(expectedAffectedRows);
    }

    [Theory]
    [InlineData("Invalid Duration")]
    [InlineData("Invalid Distance")]
    [InlineData("Invalid Date")]
    public async Task Run_WhenRunHistoryAdderThrowsInvalidArgument_ShouldReturnBadRequestWithFailureReason(
        string exceptionMessage)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new ArgumentException(exceptionMessage));

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        BadRequestObjectResult badRequestResult = result.ShouldBeOfType<BadRequestObjectResult>();
        badRequestResult.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        UploadResponse response = badRequestResult.Value.ShouldBeOfType<UploadResponse>();
        response.Message.ShouldContain("Invalid CSV");
        response.Message.ShouldContain(exceptionMessage);
        response.RowsAdded.ShouldBe(0);
    }

    [Fact]
    public async Task Run_WhenRunHistoryAdderThrowsInvalidArgument_ShouldLogCsvImportFailedAsWarning()
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new ArgumentException("Any validation error"));

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Warning, "CSV Import Failed");
    }

    [Theory]
    [InlineData("Invalid Duration")]
    [InlineData("Invalid Distance")]
    [InlineData("Invalid Date")]
    public async Task Run_WhenRunHistoryAdderThrowsInvalidArgument_ShouldLogExceptionMessageAsWarning(
        string exceptionMessage)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new ArgumentException(exceptionMessage));

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Warning, "CSV Import Failed", exceptionMessage);
    }

    [Fact]
    public async Task Run_WhenHistoryAdderThrowsGeneralException_ReturnsGeneric500InternalServerError()
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        string exceptionMessage = "Database connection failed";
        HttpRequest request = HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception(exceptionMessage));

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        ObjectResult objectResult = result.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        UploadResponse response = objectResult.Value.ShouldBeOfType<UploadResponse>();
        response.Message.ShouldBe("An unexpected error occurred");
        response.RowsAdded.ShouldBe(0);
    }

    [Fact]
    public async Task Run_WhenHistoryAdderThrowsGeneralException_LogsErrorWithException()
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        string exceptionMessage = "Database connection failed";
        HttpRequest request =
            HttpCsvRequestHelpers.CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception(exceptionMessage));

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Error, "CSV Import Failed", exceptionMessage);
    }
}
