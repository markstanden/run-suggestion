using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.DataSources.TrainingPeaks;
using RunSuggestion.Core.Unit.Tests.TestHelpers.Assertions;
using RunSuggestion.Core.Unit.Tests.TestHelpers.Creators;
using RunSuggestion.Core.Unit.Tests.TestHelpers.Doubles;

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
    public async Task PostRunHistory_WhenCalled_LogsThatRunHistoryUploadProcessHasStarted()
    {
        // Arrange
        DefaultHttpContext context = new();
        HttpRequest request = new DefaultHttpRequest(context);

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, "Run history upload started");
    }


    [Theory]
    [InlineData("Bearer fake-token-12345")]
    [InlineData("Bearer fake-token-with-different-format")]
    [InlineData("Bearer 00fake00token00abcdefghijklmnopqrstuvwxyz0123456789")]
    public async Task PostRunHistory_WhenCalled_CallsAuthenticatorWithAuthorizationHeader(string authToken)
    {
        // Arrange
        DefaultHttpContext context = new();
        HttpRequest request = new DefaultHttpRequest(context);
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
    public async Task PostRunHistory_WhenAuthenticationSucceeds_CallsHistoryAdderWithEntraId(string entraId)
    {
        // Arrange
        DefaultHttpContext context = new();
        HttpRequest request = new DefaultHttpRequest(context);
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
    public async Task PostRunHistory_WhenAuthenticationSucceeds_LogsLastFiveOfEntraId(string entraId)
    {
        // Arrange
        string expectedLastFive = entraId.Substring(entraId.Length - 5);
        DefaultHttpContext context = new();
        HttpRequest request = new DefaultHttpRequest(context);
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
    public async Task PostRunHistory_WhenAuthenticationFails_Returns401UnauthorizedResponse()
    {
        // Arrange
        int expectedStatusCode = StatusCodes.Status401Unauthorized;
        string? nullEntraId = null;
        DefaultHttpContext context = new();
        HttpRequest request = new DefaultHttpRequest(context);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(nullEntraId);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        UnauthorizedResult unauthorizedResult = result.ShouldBeOfType<UnauthorizedResult>();
        unauthorizedResult.StatusCode.ShouldBe(expectedStatusCode);
    }

    [Fact]
    public async Task PostRunHistory_WhenAuthenticationFails_DoesNotCallHistoryAdder()
    {
        // Arrange
        string? nullEntraId = null;
        DefaultHttpContext context = new();
        HttpRequest request = new DefaultHttpRequest(context);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(nullEntraId);

        // Act
        await _sut.Run(request);

        // Assert
        _mockHistoryAdder.Verify(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PostRunHistory_WhenAuthenticationFails_LogsFailedRequestAsWarning()
    {
        // Arrange
        string? nullEntraId = null;
        DefaultHttpContext context = new();
        HttpRequest request = new DefaultHttpRequest(context);
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
    public async Task PostRunHistory_WhenAuthenticationSucceedsAndCsvPresent_CsvIsPassedIntoHistoryAdder(int csvRows)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        IEnumerable<TrainingPeaksActivity> activities = TrainingPeaksActivityFakes.CreateRandomRuns(csvRows);
        string csv = TrainingPeaksCsvBuilder.CsvFromActivities(activities);
        HttpRequest request = CreateCsvUploadRequest(authToken, csv);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());

        // Act
        await _sut.Run(request);

        // Assert
        _mockHistoryAdder.Verify(x => x.AddRunHistory(It.IsAny<string>(), csv), Times.Once);
    }

    [Fact]
    public async Task PostRunHistory_WhenAuthenticationSucceedsAndValidCsvPresent_Returns200OkResult()
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        HttpRequest request = CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(0);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task PostRunHistory_WhenAuthenticationSucceedsAndValidCsvPresent_ExpectedRowsReturned(
        int expectedAffectedRows)
    {
        // Arrange
        string authToken = $"Bearer {Guid.NewGuid()}";
        HttpRequest request = CreateCsvUploadRequest(authToken, string.Empty);
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(EntraIdFakes.CreateEntraId());
        _mockHistoryAdder.Setup(x => x.AddRunHistory(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedAffectedRows);

        // Act
        IActionResult result = await _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(expectedAffectedRows);
    }

    #region TestHelpers

    /// <summary>
    /// Creates an HTTP request configured for CSV upload with 'authorization' header.
    /// </summary>
    /// <param name="authToken">The authorisation token to include in the header</param>
    /// <param name="csv">The CSV content for the request body</param>
    /// <returns>Configured HttpRequest for CSV upload</returns>
    private static HttpRequest CreateCsvUploadRequest(string authToken, string csv) =>
        HttpRequestHelper.CreateHttpRequestWithHeader("Authorization",
                                                      authToken,
                                                      "POST",
                                                      csv,
                                                      "text/csv");

    #endregion
}
