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

    public TrainingPeaksHistoryService(IUserRepository userRepository, IRunHistoryTransformer runHistoryTransformer)
    {
        _runHistoryTransformer = runHistoryTransformer; 
        _userRepository = userRepository;
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

        // TODO: merge the histories?
        
        int affectedRows = await _userRepository.AddRunEventsAsync(userId, runHistory);

        // TODO: Return affected rows
        return affectedRows;
    }
    
    
}
