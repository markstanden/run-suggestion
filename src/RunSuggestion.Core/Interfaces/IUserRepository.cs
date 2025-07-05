using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;

namespace RunSuggestion.Core.Interfaces;

public interface IUserRepository
{
    /// <summary>
    /// Creates a new user in the database
    /// </summary>
    /// <param name="entraId">The GUID provided by entra to uniquely identify this user</param>
    /// <returns>The internal userId of the user.</returns>
    Task<int> CreateUserAsync(string entraId);

    /// <summary>
    /// Retrieves a UserData object by userId from the database
    /// </summary>
    /// <param name="userId">The internal Id used to uniquely identify this user</param>
    /// <returns>The full user data associated with the user</returns>
    Task<UserData?> GetUserDataByUserIdAsync(int userId);

    /// <summary>
    /// Retrieves a UserData object by entraId from the database
    /// </summary>
    /// <param name="entraId">The external Id used to uniquely identify this user by azure entra</param>
    /// <returns>The full user data associated with the user</returns>
    Task<UserData?> GetUserDataByEntraIdAsync(string entraId);

    /// <summary>
    /// Adds run history items to a user's run history.
    /// Returns the number of affected rows.
    /// </summary>
    /// <param name="userId">The internal userId to associate the run history events with</param>
    /// <param name="runEvents">The run events to add to the user's history.</param>
    /// <returns>The number of added rows within the database</returns>
    Task<int> AddRunHistoryAsync(int userId, IEnumerable<RunEvent> runEvents);

    /// <summary>
    /// Retrieves all run events for a specific user
    /// </summary>
    /// <param name="userId">The internal userId to retrieve events for</param>
    /// <returns>Collection of run events associated with the user</returns>
    Task<IEnumerable<RunEvent>> GetRunEventsByUserIdAsync(int userId);
}
