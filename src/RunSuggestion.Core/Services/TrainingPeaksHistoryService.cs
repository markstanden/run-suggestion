using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Core.Services;

/// <summary>
/// Orchestrator of the data ingestion, the history service handles 
/// </summary>
public class TrainingPeaksHistoryService : IRunHistoryAdder
{
    private readonly IRunHistoryTransformer _runHistoryTransformer;

    public TrainingPeaksHistoryService(IRunHistoryTransformer runHistoryTransformer)
    {
        _runHistoryTransformer = runHistoryTransformer;
    }

    /// <inheritdoc />
    public async Task<int> AddRunHistory(string entraId, string historyCsv)
    {
        // TODO: Obtain user's internal Id and existing history 
        
        // TODO: Transform into IEnumerable<RunEvent>
        IEnumerable<RunEvent> runHistory = _runHistoryTransformer.Transform(historyCsv);

        // TODO: merge the histories?
        
        // TODO: Add to Database

        // TODO: Return affected rows
        return runHistory.Count();
    }
    
    
}
