namespace RunSuggestion.Shared.Constants;

public static class Errors
{
    public static class Authentication
    {
        public const string NullOrWhitespaceToken = "Invalid Token - Token cannot be null or whitespace.";
        public const string InvalidToken = "Invalid Token - Token is not a valid JWT bearer token";
    }

    public static class History
    {
        public const string NoCsvContent = "Invalid CSV content provided - Content is empty.";
        public const string InvalidCsvContent = "Invalid CSV content provided - please check the file and try again.";
    }
}
