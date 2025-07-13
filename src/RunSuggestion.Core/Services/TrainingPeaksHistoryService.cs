using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;

namespace RunSuggestion.Core.Services;

/// <summary>
/// Orchestrator of the data ingestion, the history service handles 
/// </summary>
public class TrainingPeaksHistoryService : IRunHistoryAdder
{
    private readonly IRunHistoryTransformer _runHistoryTransformer;
    private readonly IUserRepository _userRepository;
    private readonly IValidator<RunEvent> _validator;

    public TrainingPeaksHistoryService(IUserRepository userRepository, IRunHistoryTransformer runHistoryTransformer, IValidator<RunEvent> validator)
    {
        _runHistoryTransformer = runHistoryTransformer; 
        _userRepository = userRepository;
        _validator = validator;
    }

    /// <inheritdoc />
    public async Task<int> AddRunHistory(string entraId, string historyCsv)
    {
        if (string.IsNullOrWhiteSpace(entraId))
        {
            throw new ArgumentException("Invalid EntraId - cannot be null or whitespace", nameof(entraId));
        }

        if (string.IsNullOrWhiteSpace(historyCsv))
        {
            throw new ArgumentException("Invalid historyCsv - cannot be null or whitespace", nameof(historyCsv));
        }

        UserData? userData = await _userRepository.GetUserDataByEntraIdAsync(entraId);
        int userId = userData?.UserId ?? await _userRepository.CreateUserAsync(entraId);
        
        IEnumerable<RunEvent> runHistory = _runHistoryTransformer.Transform(historyCsv).ToList();
        
        _validator.Validate(runHistory);
        
        return await _userRepository.AddRunEventsAsync(userId, runHistory);
    }
    
    
}
