using Dapper;
using Microsoft.Data.Sqlite;
using RunSuggestion.Core.Interfaces;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Models.Users;

namespace RunSuggestion.Core.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SqliteConnection _connection;
    private readonly string _createUserTableSql = File.ReadAllText("Sql/CreateUsersTable.sql");
    private readonly string _createRunEventsTableSql = File.ReadAllText("Sql/CreateRunEventsTable.sql");
    private readonly string _selectRunEventsSql = File.ReadAllText("Sql/SelectRunEvents.sql");
    private readonly string _insertRunEventsSql = File.ReadAllText("Sql/InsertRunEvents.sql");
    private readonly string _insertUserSql = File.ReadAllText("Sql/InsertUser.sql");

    
    public UserRepository(string connectionString)
    {
        _connection = new SqliteConnection(connectionString);
        _connection.Open();
        InitializeDatabase();
    }
    
    private void InitializeDatabase()
    {
        _connection.Open();
        _connection.Execute(_createUserTableSql);
        _connection.Execute(_createRunEventsTableSql);
    }

    public async Task<int> CreateUserAsync(string? entraId)
    {
        try
        {
            return await _connection.ExecuteAsync(_insertUserSql, new { EntraId = entraId });
        }
        catch (SqliteException)
        {
            throw new ArgumentException("EntraID already exists");
        }
    }

    public async Task<UserData?> GetUserDataByUserIdAsync(int userId)
    {
        IEnumerable<RunEvent> runEvents = await _connection.QueryAsync<RunEvent>(
            _selectRunEventsSql, 
            new { UserId = userId });
            
        return new UserData { UserId = userId, RunHistory = runEvents.ToArray() };
    }

    public Task<UserData?> GetUserDataByEntraIdAsync(string entraId)
    {
        throw new NotImplementedException();
    }

    public async Task<int> AddRunHistoryAsync(int userId, IEnumerable<RunEvent> runEvents)
    {
        var insertParameters = runEvents.Select(runEvent => 
            new {
                UserId = userId, 
                Date = runEvent.Date, 
                Distance = runEvent.Distance, 
                Effort = runEvent.Effort, 
                Duration = runEvent.Duration
            });
        
        return await _connection.ExecuteAsync(_insertRunEventsSql, insertParameters);
    }
}