using Microsoft.Extensions.Logging;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Runs;
using RunSuggestion.Shared.Models.Users;

namespace RunSuggestion.Core.Services;

public class RecommendationService(
    ILogger<RecommendationService> logger,
    IUserRepository userRepository,
    DateTime? currentDate = null)
    : IRecommendationService
{
    private const int PrevWeek = -7;
    private const int RequiredRunHistoryWeeks = 5;

    internal const string LogMessageCalled =
        "Attempting to calculate RunRecommendation";

    internal const string LogMessageInvalidId =
        "Invalid EntraId - cannot be null or whitespace";

    internal const string LogMessageInsufficientHistory =
        "Insufficient RunEvent history provided, supplying a cautious base recommendation";

    private readonly DateTime _currentDate =
        currentDate ?? DateTime.Now;

    private readonly ILogger<RecommendationService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IUserRepository _userRepository =
        userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    /// <summary>
    /// Generates a run recommendation for the user identified by the given Entra ID.
    /// </summary>
    /// <remarks>
    /// The recommendation is based on the user's recent run history: only runs within the last 5 weeks are considered.
    /// If no recent history exists the method logs that history is insufficient but still returns a recommendation derived from the calculation helpers (which provide predefined "insufficient history" values).
    /// </remarks>
    /// <param name="entraId">The user's Entra identifier; must not be null, empty or whitespace.</param>
    /// <returns>A <see cref="RunRecommendation"/> populated with the current date and calculated Distance, Effort and Duration values.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="entraId"/> is null, empty or consists only of whitespace.</exception>
    public async Task<RunRecommendation> GetRecommendationAsync(string entraId)
    {
        _logger.LogInformation(LogMessageCalled);
        if (string.IsNullOrWhiteSpace(entraId))
        {
            _logger.LogCritical(LogMessageInvalidId);
            throw new ArgumentException(LogMessageInvalidId, nameof(entraId));
        }

        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);

        IEnumerable<RunEvent> fullRunHistory = userData?.RunHistory ?? [];
        List<RunEvent> recentRunHistory = fullRunHistory
            .Where(rh => rh.Date > _currentDate.AddDays(PrevWeek * RequiredRunHistoryWeeks))
            .ToList();

        if (IsEmptyRunHistory(recentRunHistory))
        {
            _logger.LogInformation(LogMessageInsufficientHistory);
        }

        return new RunRecommendation
        {
            Date = _currentDate.Date,
            Distance = CalculateDistance(recentRunHistory),
            Effort = CalculateEffort(recentRunHistory),
            Duration = CalculateDuration(recentRunHistory)
        };
    }

    /// <summary>
    /// User-configurable rule to calculate run duration based on run history.
    /// </summary>
    /// <param name="runEvents">The users past completed run events</param>
    /// <summary>
    /// Determines the recommended run duration based on recent run history.
    /// </summary>
    /// <param name="runEvents">Recent run events to base the recommendation on; may be null or empty.</param>
    /// <returns>
    /// A TimeSpan representing the target duration. Returns the predefined "insufficient history" duration when no run history is available; otherwise returns a 30-minute duration.
    /// </returns>
    internal TimeSpan CalculateDuration(IEnumerable<RunEvent>? runEvents)
    {
        if (IsEmptyRunHistory(runEvents))
        {
            return Runs.InsufficientHistory.RunDurationTimeSpan;
        }

        return TimeSpan.FromMinutes(30);
    }

    /// <summary>
    /// User-configurable rule to calculate run effort based on a flexible low-effort / high-effort ratio.
    /// </summary>
    /// <param name="runEvents">The users passed completed run events</param>
    /// <param name="highEffortPercentage">The target weekly percentage of runs that should be high effort</param>
    /// <summary>
    /// Determines a recommended effort level for the next run based on recent run history.
    /// </summary>
    /// <param name="runEvents">Recent run events to consider; may be null. Only runs within the last seven days are used for the high-effort comparison.</param>
    /// <param name="highEffortPercentage">
    /// Target percentage of high-effort runs to aim for in the current week.
    /// Defaults to <c>RuleConfig.Default.SafeHighEffortTargetPercentage</c>.
    /// </param>
    /// <returns>
    /// A byte code corresponding to a Run effort level:
    /// - <c>Runs.InsufficientHistory.RunEffort</c> when there is no history;
    /// - <c>Runs.EffortLevel.Recovery</c> if high-effort runs exceed the target;
    /// - <c>Runs.EffortLevel.Easy</c> if high-effort runs meet the target;
    /// - <c>Runs.EffortLevel.Strong</c> if high-effort runs are below the target.
    /// </returns>
    internal byte CalculateEffort(IEnumerable<RunEvent>? runEvents,
        int highEffortPercentage = RuleConfig.Default.SafeHighEffortTargetPercentage)
    {
        List<RunEvent> recentRunEvents = runEvents?.ToList() ?? [];
        if (IsEmptyRunHistory(recentRunEvents))
        {
            return Runs.InsufficientHistory.RunEffort;
        }

        List<RunEvent> currentWeeksRuns = recentRunEvents
            .Where(re => re.Date > _currentDate.AddDays(PrevWeek))
            .ToList();

        int highEffortCount = CalculateHighEffortCount(currentWeeksRuns);
        int targetHighEffortCount = CalculateTargetHighEffortRunQuantity(currentWeeksRuns.Count, highEffortPercentage);

        if (highEffortCount > targetHighEffortCount)
        {
            // Recommend a recovery run if the target threshold has already been exceeded
            return Runs.EffortLevel.Recovery;
        }

        if (highEffortCount == targetHighEffortCount)
        {
            // Recommend an easy run if the target threshold has been met (but not exceeded)
            return Runs.EffortLevel.Easy;
        }

        // User is ready for a hard run
        return Runs.EffortLevel.Strong;
    }

    /// <summary>
    /// Calculates the quantity of high-effort run events from the provided collection of run events.
    /// The threshold (exclusive) of what constitutes a high-effort run can be set
    /// (defaults to anything above an <see cref="Runs.EffortLevel.Easy">easy</see> run.
    /// </summary>
    /// <param name="runEvents">The collection of run events to evaluate.</param>
    /// <param name="highEffortThreshold">
    /// The threshold above which a run event's effort is considered high.
    /// Defaults to <see cref="Runs.EffortLevel.Easy"/>.\
    /// </param>
    /// <returns>The number of high-effort run events exceeding the provided threshold.</returns>
    internal static int CalculateHighEffortCount(IEnumerable<RunEvent> runEvents,
        byte highEffortThreshold = Runs.EffortLevel.Easy) =>
        runEvents.Count(re => re.Effort > highEffortThreshold);

    /// <summary>
    /// Calculates the quantity of runs that should be high effort within the current training period.
    /// </summary>
    /// <param name="recentRunCount">The number of runs conducted within the last training period</param>
    /// <param name="highEffortPercentage">The percentage of runs within the training period that should be high effort</param>
    /// <returns>The quantity of runs that should be completed using high effort</returns>
    internal static int CalculateTargetHighEffortRunQuantity(int recentRunCount, int highEffortPercentage)
    {
        int totalRunCount = recentRunCount + 1;
        return (int)Math.Floor(totalRunCount * (highEffortPercentage / 100.0));
    }

    /// <summary>
    /// User configuarble rule to calculate distance based on weekly average. 
    /// </summary>
    /// <param name="runEvents">The users passed completed run events</param>
    /// <param name="progressionPercent">The target weekly progression.</param>
    /// <summary>
    /// Calculates the recommended additional distance (metres) to reach a weekly target based on recent run history and a progression percentage.
    /// </summary>
    /// <param name="runEvents">Recent run events to base the calculation on; may be null or empty.</param>
    /// <param name="progressionPercent">
    /// Desired progression as a percentage (applied to the historic weekly average to form the target load).
    /// The value is validated by <see cref="CalculateProgressionRatio"/> and must be within the configured progression bounds.
    /// </param>
    /// <returns>
    /// The rounded difference between the target weekly load and the current weekly load (in metres).
    /// If the provided history is null or empty, returns <see cref="Runs.InsufficientHistory.RunDistanceMetres"/>.
    /// </returns>
    internal int CalculateDistance(IEnumerable<RunEvent>? runEvents,
        int progressionPercent = RuleConfig.Default.SafeProgressionPercent)
    {
        List<RunEvent> recentRunEvents = runEvents?.ToList() ?? [];
        if (IsEmptyRunHistory(recentRunEvents))
        {
            return Runs.InsufficientHistory.RunDistanceMetres;
        }

        double currentWeeklyLoad = CalculateRollingTotalLoad(recentRunEvents, _currentDate, 7);
        double previousWeeklyLoad = CalculateHistoricWeeklyAverageDistance(recentRunEvents, _currentDate);
        double targetWeeklyLoad = previousWeeklyLoad * CalculateProgressionRatio(progressionPercent);

        return (int)Math.Round(targetWeeklyLoad - currentWeeklyLoad);
    }

    /// <summary>
    /// Converts a progression percentage into a multiplier ratio
    /// throws if a value outside the safe range is provided
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
            .Select(weekNumber => CalculateRollingTotalLoad(runEvents, currentDate.AddDays(PrevWeek * weekNumber)))
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
