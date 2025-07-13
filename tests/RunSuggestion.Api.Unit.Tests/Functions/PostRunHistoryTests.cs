using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
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
}