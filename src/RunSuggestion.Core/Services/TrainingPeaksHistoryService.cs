using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Core.Services;

public class TrainingPeaksHistoryService: IRunHistoryAdder
{
    /// <summary>
    /// Adds the user's TrainingPeaks run history to the provided userid
    /// History is provided as a CSV string, from the sites export facility.
    /// </summary>
    /// <param name="userId">The User's Unique identifier</param>
    /// <param name="historyCsv">The User's run history provided as a CSV.</param>
    /// <returns>The number of records added to the user history</returns>
    public int AddRunHistory(int userId, string historyCsv)
    {
        throw new NotImplementedException();

        // TODO: Transform into IEnumerable<RunEvent>

        // TODO: Add to Database

        // TODO: Return affected rows
    }
}
