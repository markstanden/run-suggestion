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
        HttpRequest request = new DefaultHttpRequest(new DefaultHttpContext());
        
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
}