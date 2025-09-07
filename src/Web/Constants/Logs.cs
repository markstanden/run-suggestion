namespace RunSuggestion.Web.Constants;

public static class Logs
{
    public static class Upload
    {
        public const string Start = "Upload process started";
        public const string Success = "Upload process completed successfully";
        public const string Failure = "Upload process failed";
    }

    public static class Recommendation
    {
        public const string Start = "GetRecommendation process started";
        public const string Success = "GetRecommendation process completed successfully";
        public const string Failure = "GetRecommendation process failed";
    }
}
