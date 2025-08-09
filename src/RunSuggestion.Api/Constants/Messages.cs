namespace RunSuggestion.Api.Constants;

public static class Messages
{
    public static class Authentication
    {
        public const string Success = "Successfully Authenticated user";
        public const string Failure = "Failed to authenticate user.";
    }

    public static class Csv
    {
        public const string UploadStarted = "Run history upload started.";
        public const string Invalid = "Invalid CSV content";
        public const string Success = "Successfully processed CSV";
        public const string Failure = "CSV Import Failed";
    }

    public const string RequestReceived = "Run suggestion request received.";
    public const string UnexpectedError = "An unexpected error occurred";
    public const string RequestReceivedLog = "HealthCheck Endpoint recieved request";
    public const string HealthCheckResponse = "System Healthy";
}
