namespace RunSuggestion.Core.Constants;

public class DatabaseConstants
{
    public static class SqliteConnection
    {
        public const string Key = "DatabaseConnectionString";
        public const string Default = "Data Source=:memory:";
    }
}
