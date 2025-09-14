using RunSuggestion.Shared.Models.Runs;

namespace RunSuggestion.Web.Interfaces;

public interface ICsvUploadApiService
{
    /// <summary>
    /// Service to upload a csv file to the backend API,
    /// Returns The number of <see cref="RunEvent"/>s added to the user's account
    /// </summary>
    /// <param name="csvContent">
    /// The CSV file content to upload
    /// </param>
    /// <returns>
    /// An <see cref="Int32"/> representing the number of <see cref="RunEvent"/>s added to the user's account
    /// </returns>
    public Task<int> UploadAsync(string csvContent);
}
