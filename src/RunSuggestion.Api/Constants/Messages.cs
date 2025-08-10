namespace RunSuggestion.Api.Constants;

public static class Messages
{
    public static class Authentication
    {
        public const string Success = "Successfully Authenticated user";
        public const string Failure = "Failed to authenticate user.";
    }

    public static class CsvUpload
    {
        public const string RequestReceived = "Run history upload started.";
        public const string Invalid = "Invalid CSV content";
        public const string Success = "Successfully processed CSV";
        public const string Failure = "CSV Import Failed";
    }

    public static class Recommendation
    {
        public const string RequestReceived = "Run recommendation request received.";
    }

    public static class HealthCheck
    {
        public const string RequestReceived = "HealthCheck Endpoint recieved request";
        public const string Success = "System Healthy";
    }

    public const string UnexpectedError = "An unexpected error occurred";
}
