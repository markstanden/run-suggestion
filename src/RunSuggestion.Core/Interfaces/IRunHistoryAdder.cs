namespace RunSuggestion.Core.Interfaces;

public interface IRunHistoryAdder
{
    /// <summary>
    /// Adds the user's run history to the provided userid
    /// </summary>
    /// <param name="entraId">The User's Unique identifier within the authentication provider</param>
    /// <param name="history">The User's run history as a CSV</param>
    /// <returns>The number of records added to the user history</returns>
    Task<int> AddRunHistory(string entraId, string history);
}
