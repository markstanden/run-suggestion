using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.Shared.Models.Users;

namespace RunSuggestion.Core.Services;

public class RecommendationService : IRecommendationService
{
    private readonly DateTime _currentDate;
    private readonly ILogger<RecommendationService> _logger;
    private readonly IUserRepository _userRepository;

    public RecommendationService(ILogger<RecommendationService> logger, IUserRepository userRepository,
        DateTime? currentDate = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _currentDate = currentDate ?? DateTime.Now;
    }

    /// <inheritdoc/>
    public async Task<RunRecommendation> GetRecommendationAsync(string entraId)
    {
        if (string.IsNullOrWhiteSpace(entraId))
        {
            throw new ArgumentException("Invalid EntraId - cannot be null or whitespace", nameof(entraId));
        }

        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);

        if (userData is null || IsEmptyRunHistory(userData.RunHistory))
        {
            return GetBaseRecommendation();
        }

        return new RunRecommendation
        {
            RunRecommendationId = 1,
            Date = _currentDate.Date,
            Distance = CalculateDistance(userData.RunHistory),
            Effort = CalculateEffort(userData.RunHistory),
            Duration = Runs.RunDistanceBaseDurationTimeSpan
        };
    }

    private RunRecommendation GetBaseRecommendation() => new()
    {
        RunRecommendationId = 1,
        Date = _currentDate.Date,
        Distance = Runs.RunDistanceBaseMetres,
        Effort = Runs.EffortLevel.Easy,
        Duration = Runs.RunDistanceBaseDurationTimeSpan
    };

    /// <summary>
    /// User configurable rule to calculate run effort based on a flexible low effort/high effort ratio.
    /// </summary>
    /// <param name="runEvents">The users passed completed run events</param>
    /// <param name="highEffortPercentage">The target weekly percentage of runs that should be high intensity</param>
    /// <returns>a rough target effort level to apply during the run</returns>
    internal byte CalculateEffort(IEnumerable<RunEvent> runEvents,
        int highEffortPercentage = RuleConfig.Default.SafeHighEffortTargetPercentage)
    {
        double highEffortTargetRatio = (double)highEffortPercentage / 100;
        byte calculatedEffort = Runs.RunEffortBase;
        return calculatedEffort;
    }

    /// <summary>
    /// User configuarble rule to calculate distance based on weekly average. 
    /// </summary>
    /// <param name="runEvents">The users passed completed run events</param>
    /// <param name="progressionPercent">The target weekly progression.</param>
    /// <returns>A rounded target distance based on the users history and target progression</returns>
    internal int CalculateDistance(IEnumerable<RunEvent> runEvents,
        int progressionPercent = RuleConfig.Default.SafeProgressionPercent)
    {
        List<RunEvent> recentRunEvents = runEvents.ToList();
        double currentWeeklyLoad = CalculateRollingTotalLoad(recentRunEvents, _currentDate, 7);
        double previousWeeklyLoad = CalculateHistoricWeeklyAverageDistance(recentRunEvents, _currentDate);
        double targetWeeklyLoad = previousWeeklyLoad * CalculateProgressionRatio(progressionPercent);

        return (int)Math.Round(targetWeeklyLoad - currentWeeklyLoad);
    }

    /// <summary>
    /// Converts a progression percentage into a multiplier ratio
    /// throws if a value outside of a safe range is provided
    /// </summary>
    /// <param name="progressionPercent">Percentage increase to convert - must be between <see cref="RuleConfig.MinProgressionPercent"/> and <see cref="RuleConfig.MaxProgressionPercent"/></param>
    /// <returns>multiplier to apply</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if progressionPercent is outside of permitted range</exception>
    internal static double CalculateProgressionRatio(int progressionPercent)
    {
        if (progressionPercent < RuleConfig.MinProgressionPercent ||
            progressionPercent > RuleConfig.MaxProgressionPercent)
        {
            string errorMessage =
                $"Progression percentage must be between {RuleConfig.MinProgressionPercent} and {RuleConfig.MaxProgressionPercent} (inclusive)";
            throw new ArgumentOutOfRangeException(nameof(progressionPercent), progressionPercent, errorMessage);
        }

        double progressionPercentage = 100 + progressionPercent;
        return progressionPercentage / 100;
    }

    /// <summary>
    /// Calculates the total load (distance) of runs within a specified time period ending at a given date.
    /// </summary>
    /// <param name="runEvents">A collection of <see cref="RunEvent"/>s to calculate the total distance from</param>
    /// <param name="endDate">The (inclusive) end date of run events to be included in the calculation</param>
    /// <param name="dayCount">The number of days before the end date to include in the calculation. Default is 7 days.</param>
    /// <returns>The total distance of completed runs within the time period.</returns>
    internal static double CalculateRollingTotalLoad(
        IEnumerable<RunEvent> runEvents,
        DateTime endDate,
        int dayCount = 7)
    {
        return runEvents
            .Where(re => re.Date <= endDate)
            .Where(re => re.Date > endDate.AddDays(-dayCount))
            .Sum(re => re.Distance);
    }

    /// <summary>
    /// Calculates the historic average weekly distance for a specified number of weeks
    /// (defaults to 4 weeks) based on the provided run events and the provided end date.
    /// Excludes any run events that have taken place in the last week
    /// </summary>
    /// <param name="runEvents">A collection of run events from which to calculate the historic weekly averages.</param>
    /// <param name="currentDate">The current date, from which the history range is calculated</param>
    /// <param name="weeks">The number of weeks to include in the averaging. Defaults to 4 weeks.</param>
    /// <returns>The calculated average weekly distance over the specified duration, excluding the most recent week</returns>
    internal static double CalculateHistoricWeeklyAverageDistance(
        IEnumerable<RunEvent> runEvents,
        DateTime currentDate,
        int weeks = 4)
    {
        return Enumerable.Range(1, weeks)
            .Select(week => CalculateRollingTotalLoad(runEvents, currentDate.AddDays(-7 * week)))
            .Average(x => x);
    }

    /// <summary>
    /// Helper method to check whether the returned run history is empty.
    /// </summary>
    /// <param name="runEvents">The nullable RunEvent collection</param>
    /// <returns>true if the run history is empty</returns>
    internal static bool IsEmptyRunHistory(IEnumerable<RunEvent>? runEvents) =>
        runEvents is null || !runEvents.Any();
}
