using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Unit.Tests.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;

namespace RunSuggestion.Api.Unit.Tests.Functions;

public class GetRunSuggestionTests
{
    private readonly Mock<ILogger<GetRunSuggestion>> _mockLogger = new();
    private readonly Mock<IAuthenticator> _mockAuthenticator = new();

    private readonly GetRunSuggestion _sut;

    public GetRunSuggestionTests()
    {
        _sut = new GetRunSuggestion(_mockLogger.Object, _mockAuthenticator.Object);
    }

    [Fact]
    public async Task Run_WhenCalled_LogsThatRunHistoryUploadProcessHasStarted()
    {
        // Arrange
        string[] expectedLogMessages = ["Run suggestion", "request received"];
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedLogMessages);
    }

    [Fact]
    public async Task Run_WithAuthHeaderNotSet_CallsAuthenticatorWithEmptyString()
    {
        // Arrange
        string expectedAuthHeader = string.Empty;
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();

        // Act
        await _sut.Run(request);

        // Assert
        _mockAuthenticator.Verify(x => x.Authenticate(expectedAuthHeader), Times.Once);
    }
}
