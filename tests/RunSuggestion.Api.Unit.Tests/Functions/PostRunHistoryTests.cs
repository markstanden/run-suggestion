using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Interfaces;

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
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Run history upload started")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
}
