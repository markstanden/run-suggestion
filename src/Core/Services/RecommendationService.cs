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

    internal const string LogMessageNegativeRunDistance = "Invalid run distance - cannot be Negative";

    private readonly DateTime _currentDate =
        currentDate ?? DateTime.Now;

    private readonly ILogger<RecommendationService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IUserRepository _userRepository =
        userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    /// <inheritdoc/>
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

        int distance = CalculateDistance(recentRunHistory);
        byte effort = distance == 0
            ? Runs.EffortLevel.Rest
            : CalculateEffort(recentRunHistory);
        TimeSpan duration = distance == 0
            ? TimeSpan.Zero
            : CalculateDuration(recentRunHistory, distance, effort);

        return new RunRecommendation
        {
            Date = _currentDate.Date,
            Distance = distance,
            Effort = effort,
            Duration = duration
        };
    }

    /// <summary>
    /// User-configurable rule to calculate run duration based on run history.
    /// </summary>
    /// <param name="runEvents">The users past completed run events</param>
    /// <param name="distanceMetres">Recommended distance in metres</param>
    /// <param name="effort">Recommended effort level</param>
    /// <returns>A target duration for the recommended run</returns>
    internal TimeSpan CalculateDuration(IEnumerable<RunEvent>? runEvents, int distanceMetres, byte effort)
    {
        if (distanceMetres < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceMetres), LogMessageNegativeRunDistance);
        }

        if (distanceMetres == 0 || effort == Runs.EffortLevel.Rest)
        {
            // Rest day recommendation - no running!
            return TimeSpan.Zero;
        }

        List<RunEvent> recentRuns = runEvents?.ToList() ?? [];
        if (IsEmptyRunHistory(recentRuns))
        {
            return Runs.InsufficientHistory.RunDurationTimeSpan(distanceMetres);
        }

        double averageMinsPerMetre = CalculateAveragePaceForEffort(recentRuns, effort);
        double totalMinutes = distanceMetres * averageMinsPerMetre;

        return TimeSpan.FromMinutes(Math.Round(totalMinutes));
    }

    /// <summary>
    /// Calculates the average pace in mins/metre for a specific effort level based on
    /// recent runs at the same effort level within the provided recent run history.
    /// If an existing run does not yet exist at the required effort level (no average),
    /// the function recursively calls itself with the next effort level down until a match is made.
    /// If no match can be found, the base run pace is returned.
    /// </summary>
    /// <param name="runEvents">Completed run events to filter</param>
    /// <param name="effort">The effort level to calculate pace for</param>
    /// <returns>Average pace in minutes per metre for the recommended effort level</returns>
    internal static double CalculateAveragePaceForEffort(List<RunEvent> runEvents, byte effort)
    {
        if (effort < 1)
        {
            return Runs.InsufficientHistory.RunPaceMinsPerKm / 1000D;
        }

        List<RunEvent> runsAtEffortLevel = runEvents
            .Where(re => re.Effort == effort)
            .Where(re => re.Distance > 0)
            .ToList();

        if (runsAtEffortLevel.Count == 0)
        {
            return CalculateAveragePaceForEffort(runEvents, (byte)(effort - 1));
        }

        return runsAtEffortLevel.Average(re => re.Duration.TotalMinutes / re.Distance);
    }

    /// <summary>
    /// User-configurable rule to calculate run effort based on a flexible low-effort / high-effort ratio.
    /// </summary>
    /// <param name="runEvents">The users passed completed run events</param>
    /// <param name="highEffortPercentage">The target weekly percentage of runs that should be high effort</param>
    /// <returns>a rough target effort level to apply during the run</returns>
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
    /// <returns>A rounded target distance based on the users history and target progression</returns>
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

        int calculatedDistance = (int)Math.Round(targetWeeklyLoad - currentWeeklyLoad);
        return Math.Max(calculatedDistance, 0);
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
