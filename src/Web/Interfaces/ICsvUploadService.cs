namespace RunSuggestion.Web.Interfaces;

public interface ICsvUploadService
{
    /// <summary>
    /// Service to upload a csv file to the backend API,
    /// Returns true if the upload was successful, false otherwise.
    /// </summary>
    /// <param name="csvContent">The CSV to upload</param>
    /// <returns>True if the CSV uploads successfully, false otherwise</returns>
    public Task<bool> Upload(string csvContent);
}
