namespace RunSuggestion.Core.Sql;

/// <summary>
/// Constant class referencing the contents of the SQL queries.
/// Removing the SQL strings from the repository and importing this way
/// allows for the SQL files to have syntax highlighting and IDE support for the SQL dialect.
/// </summary>
public static class SqlQueries
{
    /// <summary>
    /// SqlLite query to create the Users table
    /// </summary>
    public static readonly string CreateUserTableSql = GetFromFile("Sql/CreateUsersTable.sql");

    /// <summary>
    /// SqlLite query to create the RunEvents table - has a dependency on the Users table.
    /// </summary>
    public static readonly string CreateRunEventsTableSql = GetFromFile("Sql/CreateRunEventsTable.sql");

    /// <summary>
    /// SqlLite query to return a user's Account Data from a provided UserId from the Users table
    /// </summary>
    public static readonly string SelectUserDataByUserIdSql = GetFromFile("Sql/SelectUserDataByUserId.sql");

    /// <summary>
    /// SqlLite query to return a user's Account Data from a provided EntraId from the Users table
    /// </summary>
    public static readonly string SelectUserDataByEntraIdSql = GetFromFile("Sql/SelectUserDataByEntraId.sql");

    /// <summary>
    /// SqlLite query to return a user's RunEvents history from the RunEvents table
    /// </summary>
    public static readonly string SelectRunEventsSql = GetFromFile("Sql/SelectRunEvents.sql");

    /// <summary>
    /// SqlLite query to add a RunEvent to a user's history in the RunEvents table
    /// </summary>
    public static readonly string InsertRunEventsSql = GetFromFile("Sql/InsertRunEvents.sql");

    /// <summary>
    /// SqlLite query to create a new user in the Users table
    /// </summary>
    public static readonly string InsertUserSql = GetFromFile("Sql/InsertUser.sql");

    /// <summary>
    /// Utility method to read the entirety of the SQL fie and parse to a string.
    /// </summary>
    /// <param name="path">Path to the SQL resource</param>
    /// <returns>The contents of the file as a string.</returns>
    private static string GetFromFile(string path) => File.ReadAllText(path);
}