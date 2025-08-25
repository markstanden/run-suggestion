using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using RunSuggestion.TestHelpers;
using RunSuggestion.Web.Authentication;
using RunSuggestion.Web.Authentication.Models;

namespace RunSuggestion.Web.Unit.Tests.Authentication;

[TestSubject(typeof(StaticWebAppsAuthStateProvider))]
public class StaticWebAppsAuthStateProviderTest
{
    private readonly Mock<ILogger<StaticWebAppsAuthStateProvider>> _logger = new();

    # region TestHelpers

    /// <summary>
    /// Method to allow mocking of an HTTPClient response.
    /// We can't mock HTTPClient directly as it's a closed class, so we create a 'real' instance
    /// with a mocked <see cref="HttpMessageHandler">HttpMessageHandler</see>.
    /// By mocking the protected <see cref="HttpMessageHandler.SendAsync">SendAsync</see> method,
    /// and returning the passed <see cref="HttpResponseMessage"/> we can mimic the behaviour of
    /// the authentication call.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/> response to be returned by the mocked client</param>
    /// <returns>a <see cref="Mock{HttpMessageHandler}"/> instance that returns the passed <see cref="HttpMessageHandler"/> response on a call to <see cref="HttpMessageHandler.SendAsync">SendAsync</see></returns>
    private static Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpResponseMessage response)
    {
        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>())
            .ReturnsAsync(response);

        return handlerMock;
    }

    /// <summary>
    /// A custom test implementation of <see cref="HttpMessageHandler"/> that handles
    /// http responses returning a pre-prepared <see cref="HttpResponseMessage"/>.
    /// </summary>
    private class TestHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken) => Task.FromResult(response);
    }

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

    /// <summary>
    /// Convenience method to serialise the authentication DTO and return as an <see cref="HttpResponseMessage"/>
    /// </summary>
    /// <param name="dto">The <see cref="StaticWebAppsAuthDto"/> to add to the response</param>
    /// <param name="statusCode">The HTTP status code to add to the response headers</param>
    /// <returns>A <see cref="HttpResponseMessage"/> containing the serialised DTO</returns>
    private HttpResponseMessage CreateResponse(
        StaticWebAppsAuthDto dto,
        HttpStatusCode statusCode = HttpStatusCode.OK) =>
        new(statusCode)
        {
            Content = new StringContent(JsonSerializer.Serialize(dto))
        };

    #endregion

    #region Constructor Tests

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
        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        HttpClient testHttpClient = new(handlerMock.Object) { BaseAddress = Any.Url };
        StaticWebAppsAuthStateProvider sut = new(testHttpClient, _logger.Object);

        // Act
        await sut.GetAuthenticationStateAsync();

        // Assert
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
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

    #endregion
}
