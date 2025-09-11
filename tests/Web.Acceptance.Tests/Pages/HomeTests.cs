using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using RunSuggestion.Shared.EnvironmentVariables;
using RunSuggestion.Web.Constants;

namespace RunSuggestion.Web.Acceptance.Tests.Pages;

[Trait("SafeForProduction", "True")]
public class HomeTests : PageTest
{
    private readonly string _pageUrl = BaseUrl.Value;

    [Fact]
    public async Task HasExpectedPageTitle()
    {
        // Arrange
        const string expectedTitle = Site.HomePage.Title;

        // Act
        await Page.GotoAsync(_pageUrl);

        // Assert
        await Expect(Page).ToHaveTitleAsync(expectedTitle);
    }

    [Fact]
    public async Task HasExpectedDisplayedHeading()
    {
        // Arrange
        const string testId = "homepage-heading";
        const string expectedText = Site.HomePage.Heading;

        // Act
        await Page.GotoAsync(_pageUrl);
        ILocator sloganElement = Page.GetByTestId(testId);

        // Assert
        await Expect(sloganElement).ToHaveTextAsync(expectedText);
    }
}
