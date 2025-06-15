using RunSuggestion.Core.Models.Runs;

namespace RunSuggestion.Core.Interfaces;

public interface IUserRespository
{
    /// <summary>
    /// Adds run history items to a user's run history.
    /// Returns the number of affected rows.
    /// </summary>
    /// <param name="userId">The userId to associate the run history events with</param>
    /// <param name="runEvents">The run events to add to the user's history.</param>
    /// <returns></returns>
    int AddRunHistory(int userId, IEnumerable<RunEvent> runEvents);
}
