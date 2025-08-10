using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Unit.Tests.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;
using RunSuggestion.TestHelpers;

namespace RunSuggestion.Api.Unit.Tests.Functions;

[JetBrains.Annotations.TestSubject(typeof(GetRunRecommendation))]
public class GetRunRecommendationTests
{
    private readonly Mock<ILogger<GetRunRecommendation>> _mockLogger = new();
    private readonly Mock<IAuthenticator> _mockAuthenticator = new();
    private readonly Mock<IRecommendationService> _mockRecommendationService = new();

    private readonly GetRunRecommendation _sut;

    public GetRunRecommendationTests()
    {
        _sut = new GetRunRecommendation(_mockLogger.Object,
                                        _mockAuthenticator.Object,
                                        _mockRecommendationService.Object);
    }

    [Fact]
    public async Task Run_WhenCalled_LogsThatRunHistoryUploadProcessHasStarted()
    {
        // Arrange
        string expectedMessage = Messages.Recommendation.RequestReceived;
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedMessage);
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

    [Theory]
    [InlineData(Any.ShortAlphanumericString)]
    [InlineData(Any.LongAlphanumericString)]
    [InlineData(Any.LongAlphaWithSpecialCharsString)]
    public async Task Run_WhenCalledWithAuthHeaderSet_CallsAuthenticatorWithHeaderValue(string authToken)
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        request.Headers.Authorization = authToken;

        // Act
        await _sut.Run(request);

        // Assert
        _mockAuthenticator.Verify(x => x.Authenticate(authToken), Times.Once);
    }


    [Theory]
    [InlineData(Any.ShortAlphanumericString)]
    [InlineData(Any.LongAlphanumericString)]
    [InlineData(Any.LongAlphaWithSpecialCharsString)]
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

    #endregion

    #region SuggestionService

    [Theory]
    [InlineData(Any.ShortAlphanumericString)]
    [InlineData(Any.LongAlphanumericString)]
    [InlineData(Any.LongAlphaWithSpecialCharsString)]
    public async Task Run_WhenAuthenticationSucceeds_CallsSuggestionServiceWithEntraId(string entraId)
    {
        // Arrange
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(entraId);

        // Act
        await _sut.Run(request);

        // Assert
        _mockRecommendationService.Verify(x => x.GetRecommendation(It.IsAny<string>()), Times.Once);
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
        _mockRecommendationService.Verify(x => x.GetRecommendation(It.IsAny<string>()), Times.Never);
    }

    #endregion
}
