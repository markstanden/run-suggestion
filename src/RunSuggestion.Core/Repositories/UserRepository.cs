using Dapper;
using Microsoft.Data.Sqlite;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;
using RunSuggestion.Core.Sql;

namespace RunSuggestion.Core.Repositories;

/// <summary>
/// Repository class isolating Database interactions behind a single class.
/// </summary>
public class UserRepository : IUserRepository
{
    // Holding the connection as an instance variable prevents closure of the database and
    // allows the database to be accessed for the life of the class.
    // It would be more efficient to open and close the connection on read and write
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Dependency Injection constructor method to create a connection to the database and initialise.
    /// Important in our case as for the prototype we are using an in memory SqlLite database, which will not
    /// retain state between application runs.
    /// This requires us to create the table each run, and pre-seed with fake user data to allow application testing.
    /// </summary>
    /// <param name="connectionString">Dependency injected connection string allows for different databases to be used in testing</param>
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
    /// <see href="https://www.learndapper.com/dapper-query/selecting-scalar-values#dapper-executescalarasync">
    /// Dapper's ExecuteScalarAsync method on the official documentation page
    /// </see>
    /// <exception cref="ArgumentException">Thrown when EntraID already exists</exception>
    public async Task<int> CreateUserAsync(string entraId)
    {
        // Throw early if EntraId is obviously invalid.
        ArgumentException.ThrowIfNullOrWhiteSpace(entraId, nameof(entraId));

        try
        {
            // Dapper's ExecuteScalar method returns a single value from the SQL query.
            return await _connection.ExecuteScalarAsync<int>(SqlQueries.InsertUserSql, new { EntraId = entraId });
        }
        catch (SqliteException)
        {
            throw new ArgumentException("EntraID already exists");
        }
    }

    /// <inheritdoc />
    /// Dapper implementation notes:
    /// QuerySingleOrDefaultAsync will return null if a record is not matched,
    /// and will throw if more than one record is matched, which would indicate a data integrity issue in our case
    /// <see href="https://www.learndapper.com/dapper-query/selecting-single-rows">
    /// Dapper's single row query options on the official documentation page
    /// </see>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if UserID is invalid</exception>
    public async Task<UserData?> GetUserDataByUserIdAsync(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId), userId, "Invalid UserId - must be a positive integer");
        }
        
        dynamic? queryResult = await _connection.QuerySingleOrDefaultAsync(
            SqlQueries.SelectUserDataByUserIdSql,
            new { UserId = userId });

        if (queryResult?.UserId is null)
        {
            return null;
        }

        return new UserData
        {
            UserId = (int)queryResult.UserId,
            EntraId = queryResult.EntraId,
            RunHistory = await GetRunEventsByUserIdAsync(userId)
        };
    }

    /// <inheritdoc />
    /// Dapper implementation notes:
    /// QuerySingleOrDefaultAsync will return null if a record is not matched,
    /// and will throw if more than one record is matched, which would indicate a data integrity issue in our case
    /// <see href="https://www.learndapper.com/dapper-query/selecting-single-rows">
    /// Dapper's single row query options on the official documentation page
    /// </see>
    /// <exception cref="ArgumentException">Thrown if the EntraId is invalid</exception>
    public async Task<UserData?> GetUserDataByEntraIdAsync(string entraId)
    {
        if (string.IsNullOrWhiteSpace(entraId))
        {
            throw new ArgumentException("Invalid EntraId - cannot be null or whitespace", nameof(entraId));
        }
        
        dynamic? queryResult = await _connection.QuerySingleOrDefaultAsync(
            SqlQueries.SelectUserDataByEntraIdSql,
            new { EntraId = entraId });

        if (queryResult?.UserId is null)
        {
            return null;
        }
        
        int resultUserId = (int)queryResult.UserId;

        return new UserData
        {
            UserId = resultUserId,
            EntraId = queryResult.EntraId,
            RunHistory = await GetRunEventsByUserIdAsync(resultUserId)
        };
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">Thrown if UserID is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown if runEvents is null</exception>
    public async Task<int> AddRunEventsAsync(int userId, IEnumerable<RunEvent?> runEvents)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId), userId, "Invalid UserId - must be a positive integer");
        }

        ArgumentNullException.ThrowIfNull(runEvents, nameof(runEvents));

        var insertParameters = runEvents
            .Where(runEvent => runEvent is not null)
            .Select(runEvent =>
            new
            {
                UserId = userId,
                Date = runEvent?.Date,
                Distance = runEvent?.Distance,
                Effort = runEvent?.Effort,
                Duration = runEvent?.Duration.Ticks
            });

        try
        {
            return await _connection.ExecuteAsync(SqlQueries.InsertRunEventsSql, insertParameters);
        }
        catch (SqliteException)
        {
            throw new ArgumentException("Required RunEvent data is missing");
        }
    }

    /// <inheritdoc />
    /// <see href="https://www.learndapper.com/dapper-query/selecting-single-rows#dapper-queryasync">
    /// Dapper's QueryAsync method on the official documentation page
    /// </see>
    public async Task<IEnumerable<RunEvent>> GetRunEventsByUserIdAsync(int userId)
    {
        // Dapper's QueryAsync method returns a collection of the database model
        var queryResult = await _connection.QueryAsync(
            SqlQueries.SelectRunEventsSql,
            new { UserId = userId });

        // Manually convert between the database model and the domain model.
        // We need to keep this in line with the database schema.
        IEnumerable<RunEvent> runEvents = queryResult.Select(resultRow => new RunEvent
        {
            RunEventId = (int)resultRow.RunEventId,
            Date = DateTime.Parse(resultRow.Date),
            Distance = (int)resultRow.Distance,
            Effort = (byte)resultRow.Effort,
            Duration = TimeSpan.FromTicks(resultRow.Duration)
        });

        return runEvents;
    }
}