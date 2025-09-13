namespace RunSuggestion.Web.Constants;

public static class Status
{
    public const string Uploading = "Uploading run history...";
    public const string GettingRecommendation = "Getting recommendation...";
    public const string UploadSuccess = "Upload completed successfully.";
    public const string UploadFailed = "Upload failed. Please try again.";

    public static string UploadCompleted(int rowsAdded) =>
        $"Upload completed successfully. {rowsAdded} runs added.";
}
