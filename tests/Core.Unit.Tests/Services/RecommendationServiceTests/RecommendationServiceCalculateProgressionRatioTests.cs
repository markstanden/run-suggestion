using RunSuggestion.Core.Services;
using RunSuggestion.Shared.Constants;

namespace RunSuggestion.Core.Unit.Tests.Services.RecommendationServiceTests;

[TestSubject(typeof(RecommendationService))]
public class RecommendationServiceCalculateProgressionRatioTests
{
    [Theory]
    [InlineData(-10)]
    [InlineData(999)]
    [InlineData(RuleConfig.MinProgressionPercent - 1)]
    [InlineData(RuleConfig.MaxProgressionPercent + 1)]
    public void CalculateProgressionRatio_WithOneRemainingRunAndProvidedPercentageProgression_ProvidesExpectedDistance(
        int invalidProgressionPercentage)
    {
        // Act
        Action withInvalidProgressionPercentage =
            () => RecommendationService.CalculateProgressionRatio(invalidProgressionPercentage);

        // Assert
        Exception ex =
            withInvalidProgressionPercentage.ShouldThrow<ArgumentException>();
        ex.Message.ShouldContain("progression percentage");
        ex.Message.ShouldContain($"{invalidProgressionPercentage}");
        ex.Message.ShouldContain($"{RuleConfig.MinProgressionPercent}");
        ex.Message.ShouldContain($"{RuleConfig.MaxProgressionPercent}");
    }

    [Theory]
    [InlineData(RuleConfig.MinProgressionPercent)]
    [InlineData(RuleConfig.MinProgressionPercent + 1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(RuleConfig.MaxProgressionPercent - 1)]
    [InlineData(RuleConfig.MaxProgressionPercent)]
    public void CalculateProgressionRatio_WithValidInputPercentage_ProvidesValidRatio(int progressionPercantage)
    {
        // Arrange
        double expectedMin = 1.0D; // 0% progression ratio
        double expectedMax = 1.2D; // 20% progression ratio

        // Act
        double result = RecommendationService.CalculateProgressionRatio(progressionPercantage);

        // Assert
        result.ShouldBeInRange(expectedMin, expectedMax);
    }

    [Theory]
    [InlineData(0, 1.00D)]
    [InlineData(5, 1.05D)]
    [InlineData(10, 1.10D)]
    [InlineData(15, 1.15D)]
    [InlineData(20, 1.20D)]
    public void CalculateProgressionRatio_WithValidInputPercentage_ProvidesExpectedRatio(int progressionPercantage,
        double expectedRatio)
    {
        // Act
        double result = RecommendationService.CalculateProgressionRatio(progressionPercantage);

        // Assert
        result.ShouldBe(expectedRatio);
    }
}
