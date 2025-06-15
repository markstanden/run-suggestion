using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Core.Services;

public class RunHistoryService: IRunHistoryAdder
{
    /// <summary>
    /// Adds the user's run history to the provided userid
    /// History is provided as a CSV string.
    /// </summary>
    /// <param name="userId">The User's Unique identifier</param>
    /// <param name="history">The User's run history provided as a CSV.</param>
    /// <returns>The number of records added to the user history</returns>
    public int AddRunHistory(int userId, string history)
    {
        throw new NotImplementedException();

        // TODO: Transform into IEnumerable<RunEvent>

        // TODO: Add to Database

        // TODO: Return affected rows
    }
}
