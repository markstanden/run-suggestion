using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Functions;
using RunSuggestion.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Api.Unit.Tests.Functions;

[TestSubject(typeof(GetHealthCheck))]
public class GetHealthCheckTests
{
    private readonly Mock<ILogger<GetHealthCheck>> _mockLogger = new();

    private readonly GetHealthCheck _sut;

    public GetHealthCheckTests()
    {
        _sut = new GetHealthCheck(_mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "logger";
        ILogger<GetHealthCheck> nullLogger = null!;

        // Act
        Func<GetHealthCheck> withNullLoggerArgument = () => new GetHealthCheck(nullLogger);

        // Assert
        ArgumentNullException ex = withNullLoggerArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    #endregion

    #region Request Logging

    [Fact]
    public void Run_WhenCalled_LogsRequestReceived()
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        string expectedMessage = Messages.HealthCheck.RequestReceived;

        // Act
        _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedMessage);
    }

    #endregion

    #region Response

    [Fact]
    public void Run_WhenCalled_Returns200OkResult()
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();

        // Act
        IActionResult result = _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Fact]
    public void Run_WhenCalled_ReturnsHealthyMessage()
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        string expectedMessage = Messages.HealthCheck.Success;

        // Act
        IActionResult result = _sut.Run(request);

        // Assert
        OkObjectResult okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBe(expectedMessage);
    }

    #endregion
}
