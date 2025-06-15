namespace RunSuggestion.Core.Interfaces;

public interface IRunHistoryAdder
{
    /// <summary>
    /// Adds the user's run history to the provided userid
    /// </summary>
    /// <param name="userId">The User's Unique identifier</param>
    /// <param name="history">The User's run history</param>
    /// <returns>The number of records added to the user history</returns>
    int AddRunHistory(int userId, string history);
}
