using System.Globalization;

namespace RunSuggestion.Web.Constants;

public static class Status
{
    public static class Upload
    {
        public const string Start = "Uploading run history...";

        public const string Failure = "Upload failed. Please try again.";

        public static string Success(int rowsAdded) =>
            $"Upload completed successfully. {rowsAdded} runs added.";
    }

    public static class Recommendation
    {
        public const string Start = "Getting recommendation...";

        public const string Failure = "Recommendation request failed. Please try again.";

        public static string Success(int distance, byte effort, TimeSpan duration) =>
            $"Recommendation received successfully: Distance {distance.ToString()}, Effort: {effort.ToString()}, Duration: {duration.TotalMinutes.ToString(CultureInfo.InvariantCulture)}";
    }
}
