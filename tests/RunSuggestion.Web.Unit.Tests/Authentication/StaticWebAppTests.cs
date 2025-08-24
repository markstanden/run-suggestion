using RunSuggestion.TestHelpers;
using RunSuggestion.Web.Authentication;

namespace RunSuggestion.Web.Unit.Tests.Authentication;

[TestSubject(typeof(StaticWebApp))]
public class StaticWebAppTests
{
    #region LoginPath Tests

    [Theory]
    [InlineData(Any.ShortAlphanumericString)]
    [InlineData(Any.LongAlphanumericString)]
    public void LoginPath_WithValidRedirect_ReturnsPathWithRedirectQuery(string redirect)
    {
        // Arrange
        string expectedPath = $"{StaticWebApp.LoginBasePath}/{StaticWebApp.LoginRedirectQuery}={redirect}";

        //Act
        string path = StaticWebApp.LoginPath(redirect);

        //Assert
        path.ShouldBe(expectedPath);
    }

    [Fact]
    public void LoginPath_WithNullRedirect_ReturnsPathWithoutRedirectQuery()
    {
        // Arrange
        string redirect = null!;
        string expectedPath = StaticWebApp.LoginBasePath;

        //Act
        string path = StaticWebApp.LoginPath(redirect);

        //Assert
        path.ShouldBe(expectedPath);
    }

    [Theory]
    [InlineData(Any.ShortAlphaWithSpecialCharsString)]
    [InlineData(Any.LongAlphaWithSpecialCharsString)]
    public void LoginPath_WithRedirectContainingSpecialCharacters_ReturnsPathWithRedirectUrlEncoded(string redirect)
    {
        // Arrange
        string urlEncodedRedirect = Uri.EscapeDataString(redirect);
        string expectedPath = $"{StaticWebApp.LoginBasePath}/{StaticWebApp.LoginRedirectQuery}={urlEncodedRedirect}";

        //Act
        string path = StaticWebApp.LoginPath(redirect);

        //Assert
        path.ShouldBe(expectedPath);
    }

    #endregion

    #region LogoutPath Tests

    [Theory]
    [InlineData(Any.ShortAlphanumericString)]
    [InlineData(Any.LongAlphanumericString)]
    public void LogoutPath_WithValidRedirect_ReturnsPathWithRedirectQuery(string redirect)
    {
        // Arrange
        string expectedPath = $"{StaticWebApp.LogoutBasePath}/{StaticWebApp.LogoutRedirectQuery}={redirect}";

        //Act
        string path = StaticWebApp.LogoutPath(redirect);

        //Assert
        path.ShouldBe(expectedPath);
    }

    [Fact]
    public void LogoutPath_WithNullRedirect_ReturnsPathWithoutRedirectQuery()
    {
        // Arrange
        string redirect = null!;
        string expectedPath = StaticWebApp.LogoutBasePath;

        //Act
        string path = StaticWebApp.LogoutPath(redirect);

        //Assert
        path.ShouldBe(expectedPath);
    }

    [Theory]
    [InlineData(Any.ShortAlphaWithSpecialCharsString)]
    [InlineData(Any.LongAlphaWithSpecialCharsString)]
    public void LogoutPath_WithRedirectContainingSpecialCharacters_ReturnsPathWithRedirectUrlEncoded(string redirect)
    {
        // Arrange
        string urlEncodedRedirect = Uri.EscapeDataString(redirect);
        string expectedPath = $"{StaticWebApp.LogoutBasePath}/{StaticWebApp.LogoutRedirectQuery}={urlEncodedRedirect}";

        //Act
        string path = StaticWebApp.LogoutPath(redirect);

        //Assert
        path.ShouldBe(expectedPath);
    }

    #endregion
}
