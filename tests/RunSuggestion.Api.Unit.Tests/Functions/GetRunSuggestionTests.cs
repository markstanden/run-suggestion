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

    #region Authentication

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

    #endregion
}
