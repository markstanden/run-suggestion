using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Users;
using RunSuggestion.Shared.Models.Runs;

namespace RunSuggestion.Core.Services;

/// <summary>
/// Orchestrator of the data ingestion, the history service handles creation of new users if required,
/// and transforms and adds run history.
/// </summary>
public class TrainingPeaksHistoryService : IRunHistoryAdder
{
    private readonly IRunHistoryTransformer _runHistoryTransformer;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<RunEvent> _validator;

    public TrainingPeaksHistoryService(IUserRepository userRepository, IRunHistoryTransformer runHistoryTransformer,
        IValidator<RunEvent> validator)
    {
        _runHistoryTransformer =
            runHistoryTransformer ?? throw new ArgumentNullException(nameof(runHistoryTransformer));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <inheritdoc />
    public async Task<int> AddRunHistory(string entraId, string history)
    {
        if (string.IsNullOrWhiteSpace(entraId))
        {
            throw new ArgumentException("Invalid EntraId - cannot be null or whitespace", nameof(entraId));
        }

        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);
        int userId = userData?.UserId ?? await _userRepository.CreateUserAsync(entraId);

        if (string.IsNullOrWhiteSpace(history))
        {
            throw new ArgumentException("Invalid historyCsv - cannot be null or whitespace", nameof(history));
        }

        IEnumerable<RunEvent> runHistory = _runHistoryTransformer.Transform(history).ToList();

        IEnumerable<string> errors = _validator.Validate(runHistory).ToList();
        if (errors.Any())
        {
            throw new ArgumentException(string.Join(Environment.NewLine, errors));
        }

        return await _userRepository.AddRunEventsAsync(userId, runHistory);
    }
}
