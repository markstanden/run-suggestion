using Dapper;
using Microsoft.Data.Sqlite;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;
using RunSuggestion.Core.Sql;

namespace RunSuggestion.Core.Repositories;

public class UserRepository : IUserRepository
{
    // Holding the connection as an instance variable prevents closure of the database and
    // allows the database to be accessed for the life of the class.
    // It would be more efficient to open and close the connection on read and write
    private readonly SqliteConnection _connection;




    public UserRepository(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
        InitializeDatabase();
    }

    /// <summary>
    /// Initialises the in memory database - creating user, runEvent and recommendationHistory tables.
    /// </summary>
    private void InitializeDatabase()
    {
        _connection.Open();
        _connection.Execute(SqlQueries.CreateUserTableSql);
        _connection.Execute(SqlQueries.CreateRunEventsTableSql);
    }

    // inheritdoc gets documentation from the interface to prevent documentation duplication
    /// <inheritdoc />
    /// <exception cref="ArgumentException">Thrown when EntraID already exists</exception>
    public async Task<int> CreateUserAsync(string? entraId)
    {
        try
        {
            return await _connection.ExecuteAsync(SqlQueries.InsertUserSql, new { EntraId = entraId });
        }
        catch (SqliteException)
        {
            throw new ArgumentException("EntraID already exists");
        }
    }

    /// <inheritdoc />
    public async Task<UserData?> GetUserDataByUserIdAsync(int userId)
    {
        IEnumerable<RunEvent> runEvents = await _connection.QueryAsync<RunEvent>(
            SqlQueries.SelectRunEventsSql,
            new { UserId = userId });

        return new UserData { UserId = userId, RunHistory = runEvents.ToArray() };
    }

    /// <inheritdoc />
    public Task<UserData?> GetUserDataByEntraIdAsync(string entraId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<int> AddRunHistoryAsync(int userId, IEnumerable<RunEvent> runEvents)
    {
        var insertParameters = runEvents.Select(runEvent =>
            new
            {
                UserId = userId,
                Date = runEvent.Date,
                Distance = runEvent.Distance,
                Effort = runEvent.Effort,
                Duration = runEvent.Duration
            });

        return await _connection.ExecuteAsync(SqlQueries.InsertRunEventsSql, insertParameters);
    }
}