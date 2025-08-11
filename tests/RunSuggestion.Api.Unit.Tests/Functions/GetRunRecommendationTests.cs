using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RunSuggestion.Api.Constants;
using RunSuggestion.Api.Extensions;
using RunSuggestion.Api.Functions;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Unit.Tests.TestHelpers.Assertions;
using RunSuggestion.TestHelpers.Creators;
using RunSuggestion.TestHelpers;

namespace RunSuggestion.Api.Unit.Tests.Functions;

[TestSubject(typeof(GetRunRecommendation))]
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

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "logger";
        ILogger<GetRunRecommendation> nullLogger = null!;

        // Act
        Func<GetRunRecommendation> withNullLoggerArgument = () => new GetRunRecommendation(
            nullLogger,
            _mockAuthenticator.Object,
            _mockRecommendationService.Object);

        // Assert
        ArgumentNullException ex = withNullLoggerArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    [Fact]
    public void Constructor_WithNullAuthenticator_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "authenticator";
        IAuthenticator nullAuthenticator = null!;

        // Act
        Func<GetRunRecommendation> withNullAuthenticatorArgument = () => new GetRunRecommendation(
            _mockLogger.Object,
            nullAuthenticator,
            _mockRecommendationService.Object);

        // Assert
        ArgumentNullException ex = withNullAuthenticatorArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    [Fact]
    public void Constructor_WithNullRecommendationService_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "recommendationService";
        IRecommendationService nullRecommendationService = null!;

        // Act
        Func<GetRunRecommendation> withNullRecommendationServiceArgument = () => new GetRunRecommendation(
            _mockLogger.Object,
            _mockAuthenticator.Object,
            nullRecommendationService);

        // Assert
        ArgumentNullException ex = withNullRecommendationServiceArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    #endregion

    #region Request Logging

    [Fact]
    public async Task Run_WhenCalled_LogsThatRunHistoryUploadProcessHasStarted()
    {
        // Arrange
        const string expectedMessage = Messages.Recommendation.RequestReceived;
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information, expectedMessage);
    }

    #endregion

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
    [InlineData(Any.FourCharString, 4)]
    [InlineData(Any.FiveCharString, 5)]
    [InlineData(Any.SixCharString, 5)]
    [InlineData(Any.LongAlphanumericString, 5)]
    [InlineData(Any.LongAlphaWithSpecialCharsString, 5)]
    public async Task Run_WhenAuthenticationSucceeds_LogsLastFiveOfEntraId(string entraId, int expectedLength)
    {
        // Arrange
        const string expectedMessage = Messages.Authentication.Success;
        string expectedLastFive = entraId.LastFiveChars();
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(entraId);

        // Act
        await _sut.Run(request);

        // Assert
        expectedLastFive.Length.ShouldBe(expectedLength);
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Information,
                                         expectedMessage,
                                         expectedLastFive);
    }


    [Fact]
    public async Task Run_WhenAuthenticationFails_LogsFailedRequestAsWarning()
    {
        // Arrange
        const string expectedMessage = Messages.Authentication.Failure;
        string? nullEntraId = null;
        HttpRequest request = HttpRequestHelper.CreateHttpRequest();
        _mockAuthenticator.Setup(x => x.Authenticate(It.IsAny<string>()))
            .Returns(nullEntraId);

        // Act
        await _sut.Run(request);

        // Assert
        _mockLogger.ShouldHaveLoggedOnce(LogLevel.Warning, expectedMessage);
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
    public async Task Run_WhenAuthenticationFails_DoesNotCallSuggestionService()
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
