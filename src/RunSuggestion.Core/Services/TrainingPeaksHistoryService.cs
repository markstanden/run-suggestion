using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Core.Services;

public class TrainingPeaksHistoryService : IRunHistoryAdder
{
    private readonly IRunHistoryTransformer _runHistoryTransformer;

    public TrainingPeaksHistoryService(IRunHistoryTransformer runHistoryTransformer)
    {
        _runHistoryTransformer = runHistoryTransformer;
    }

    /// <summary>
    /// Adds the user's TrainingPeaks run history to the provided userid
    /// History is provided as a CSV string, from the sites export facility.
    /// </summary>
    /// <param name="userId">The User's Unique identifier</param>
    /// <param name="historyCsv">The User's run history provided as a CSV.</param>
    /// <returns>The number of records added to the user history</returns>
    public async Task<int> AddRunHistory(int userId, string historyCsv)
    {
        // TODO: Transform into IEnumerable<RunEvent>
        IEnumerable<RunEvent> runHistory = _runHistoryTransformer.Transform(historyCsv);

        // TODO: Add to Database

        // TODO: Return affected rows
        return runHistory.Count();
    }
    
    
}
