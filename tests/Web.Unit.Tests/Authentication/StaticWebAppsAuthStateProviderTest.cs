using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RunSuggestion.Shared.Models.Auth;
using RunSuggestion.TestHelpers;
using RunSuggestion.Web.Authentication;
using static RunSuggestion.TestHelpers.Creators.HttpTestHelpers;

namespace RunSuggestion.Web.Unit.Tests.Authentication;

[TestSubject(typeof(StaticWebAppsAuthStateProvider))]
public class StaticWebAppsAuthStateProviderTest
{
    private readonly Mock<ILogger<StaticWebAppsAuthStateProvider>> _logger = new();

    # region TestHelpers

    /// <summary>
    /// Factory method returning a prepared SUT that returns the provided <see cref="HttpResponseMessage"/> response on all
    /// calls to the HttpClient.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/> response to be returned by the authentication Http request within the client.</param>
    /// <returns>the prepared SUT</returns>
    private StaticWebAppsAuthStateProvider CreateSut(HttpResponseMessage response)
    {
        HttpMessageHandler testHttpMessageHandler = new TestHttpMessageHandler(response);
        HttpClient testHttpClient = new(testHttpMessageHandler) { BaseAddress = Any.Url };
        StaticWebAppsAuthStateProvider sut = new(testHttpClient, _logger.Object);
        return sut;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "httpClient";
        HttpClient nullHttpClient = null!;

        // Act
        Func<StaticWebAppsAuthStateProvider> withNullHttpClient =
            () => new StaticWebAppsAuthStateProvider(nullHttpClient, _logger.Object);

        // Assert
        ArgumentNullException ex = withNullHttpClient.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        const string expectedParamName = "logger";
        ILogger<StaticWebAppsAuthStateProvider> nullLogger = null!;
        Mock<HttpMessageHandler> mockHandler = new();
        HttpClient testHttpClient = new(mockHandler.Object);

        // Act
        Func<StaticWebAppsAuthStateProvider> withNullLoggerArgument =
            () => new StaticWebAppsAuthStateProvider(testHttpClient, nullLogger);

        // Assert
        ArgumentNullException ex = withNullLoggerArgument.ShouldThrow<ArgumentNullException>();
        ex.ParamName.ShouldBe(expectedParamName);
    }

    #endregion

    #region GetAuthenticationStateAsync Tests

    [Fact]
    public async Task GetAuthenticationStateAsync_Always_CallsAuthenticationEndpoint()
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = Any.String,
            IdentityProvider = Any.String,
            UserDetails = Any.String
        };
        HttpResponseMessage response = CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = principal });
        Mock<HttpMessageHandler> mockHandler = new();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        HttpClient testHttpClient = new(mockHandler.Object) { BaseAddress = Any.Url };
        StaticWebAppsAuthStateProvider sut = new(testHttpClient, _logger.Object);

        // Act
        await sut.GetAuthenticationStateAsync();

        // Assert
        mockHandler
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.BadRequest)]
    public void GetAuthenticationStateAsync_WithHttpError_ReturnsFailedAuthenticationState(HttpStatusCode statusCode)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = Any.String,
            IdentityProvider = Any.String,
            UserDetails = Any.String
        };
        HttpResponseMessage response =
            CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = principal }, statusCode);
        StaticWebAppsAuthStateProvider sut = CreateSut(response);

        // Act
        AuthenticationState authenticationState = sut.GetAuthenticationStateAsync().Result;

        // Assert
        authenticationState.User.ShouldNotBeNull();
        authenticationState.User.Identity.ShouldNotBeNull();
        authenticationState.User.Identity.IsAuthenticated.ShouldBeFalse();
    }

    [Fact]
    public void GetAuthenticationStateAsync_WithValidToken_ReturnsExpectedAuthenticationState()
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = Any.String,
            IdentityProvider = Any.String,
            UserDetails = Any.String
        };
        HttpResponseMessage response = CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = principal });
        StaticWebAppsAuthStateProvider sut = CreateSut(response);

        // Act
        AuthenticationState authenticationState = sut.GetAuthenticationStateAsync().Result;

        // Assert
        authenticationState.User.ShouldNotBeNull();
        authenticationState.User.Identity.ShouldNotBeNull();
        authenticationState.User.Identity.IsAuthenticated.ShouldBeTrue();
    }

    [Fact]
    public void GetAuthenticationStateAsync_WithInvalidToken_ReturnsFailedAuthenticationState()
    {
        // Arrange
        SwaClientPrincipal nullPrinciple = null!;
        HttpResponseMessage response = CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = nullPrinciple });
        StaticWebAppsAuthStateProvider sut = CreateSut(response);

        // Act
        AuthenticationState authenticationState = sut.GetAuthenticationStateAsync().Result;

        // Assert
        authenticationState.ShouldNotBeNull();
        authenticationState.User.ShouldNotBeNull();
        authenticationState.User.Identity.ShouldNotBeNull();
        authenticationState.User.Identity.IsAuthenticated.ShouldBeFalse();
    }

    [Theory]
    [InlineData(Any.String)]
    [InlineData(Any.LongAlphanumericString)]
    [InlineData(Any.ShortAlphanumericString)]
    public async Task GetAuthenticationStateAsync_WithTokenWithValidUserId_ReturnsUserIdInClaims(
        string userId)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = userId,
            IdentityProvider = Any.String,
            UserDetails = Any.String
        };
        HttpResponseMessage response = CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = principal });
        StaticWebAppsAuthStateProvider sut = CreateSut(response);

        // Act
        AuthenticationState authenticationState = await sut.GetAuthenticationStateAsync();

        // Assert
        authenticationState.User.ShouldNotBeNull();
        authenticationState.User.Identity.ShouldNotBeNull();
        authenticationState.User.HasClaim(ClaimTypes.NameIdentifier, userId).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("       ")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    public void GetAuthenticationStateAsync_WithTokenWithInvalidUserId_ReturnsFailedAuthenticationState(
        string invalidUserId)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = invalidUserId,
            IdentityProvider = Any.String,
            UserDetails = Any.String
        };
        HttpResponseMessage response = CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = principal });
        StaticWebAppsAuthStateProvider sut = CreateSut(response);

        // Act
        AuthenticationState authenticationState = sut.GetAuthenticationStateAsync().Result;

        // Assert
        authenticationState.User.ShouldNotBeNull();
        authenticationState.User.Identity.ShouldNotBeNull();
        authenticationState.User.Identity.IsAuthenticated.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("       ")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    public void
        GetAuthenticationStateAsync_WithTokenWithInvalidIdentityProvider_ReturnsDefaultIdentityProviderInAuthenticationState(
            string invalidIdentityProvider)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = Any.String,
            IdentityProvider = invalidIdentityProvider,
            UserDetails = Any.String
        };
        HttpResponseMessage response = CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = principal });
        StaticWebAppsAuthStateProvider sut = CreateSut(response);

        // Act
        AuthenticationState authenticationState = sut.GetAuthenticationStateAsync().Result;

        // Assert
        authenticationState.User.ShouldNotBeNull();
        authenticationState.User.Identity.ShouldNotBeNull();
        authenticationState.User.Identity.IsAuthenticated.ShouldBeTrue();
        authenticationState.User
            .HasClaim(StaticWebAppsAuthStateProvider.ClaimTypeIdentityProvider,
                      StaticWebAppsAuthStateProvider.UnknownAuthProvider)
            .ShouldBeTrue();
    }

    [Theory]
    [InlineData(Any.String)]
    [InlineData(Any.LongAlphanumericString)]
    [InlineData(Any.ShortAlphanumericString)]
    public async Task GetAuthenticationStateAsync_WithTokenWithValidUserName_ReturnsUserNameIdentifierInClaims(
        string userName)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = Any.String,
            IdentityProvider = Any.String,
            UserDetails = userName
        };
        HttpResponseMessage response = CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = principal });
        StaticWebAppsAuthStateProvider sut = CreateSut(response);

        // Act
        AuthenticationState authenticationState = await sut.GetAuthenticationStateAsync();

        // Assert
        authenticationState.User.ShouldNotBeNull();
        authenticationState.User.Identity.ShouldNotBeNull();
        authenticationState.User.HasClaim(ClaimTypes.Name, userName).ShouldBeTrue();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("       ")]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    public void
        GetAuthenticationStateAsync_WithTokenWithInvalidUserDetails_ReturnsDefaultAnonymousUserHandleInAuthenticationState(
            string invalidUserDetails)
    {
        // Arrange
        SwaClientPrincipal principal = new()
        {
            UserId = Any.String,
            IdentityProvider = Any.String,
            UserDetails = invalidUserDetails
        };
        HttpResponseMessage response = CreateResponse(new StaticWebAppsAuthDto { ClientPrincipal = principal });
        StaticWebAppsAuthStateProvider sut = CreateSut(response);

        // Act
        AuthenticationState authenticationState = sut.GetAuthenticationStateAsync().Result;

        // Assert
        authenticationState.User.ShouldNotBeNull();
        authenticationState.User.Identity.ShouldNotBeNull();
        authenticationState.User.Identity.IsAuthenticated.ShouldBeTrue();
        authenticationState.User.Identity.Name.ShouldBe(StaticWebAppsAuthStateProvider.DefaultUserName);
    }

    #endregion
}
