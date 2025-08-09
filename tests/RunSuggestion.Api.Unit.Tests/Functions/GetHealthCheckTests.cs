using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Unit.Tests.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Api.Unit.Tests.Functions;

[JetBrains.Annotations.TestSubject(typeof(GetHealthCheck))]
public class GetHealthCheckTests
{
    private readonly Mock<ILogger<GetHealthCheck>> _mockLogger = new();

    private readonly GetHealthCheck _sut;

    public GetHealthCheckTests()
    {
        _sut = new GetHealthCheck(_mockLogger.Object);
    }

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
}
