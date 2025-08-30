using RunSuggestion.Shared.Constants;
using RunSuggestion.Web.Constants;
using RunSuggestion.Web.Interfaces;

namespace RunSuggestion.Web.Services;

public class CsvUploadService(ILogger<CsvUploadService> logger, HttpClient httpClient) : ICsvUploadService
{
    private readonly ILogger<CsvUploadService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<bool> Upload(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            throw new ArgumentException(Errors.Upload.NoCsvContent, nameof(csvContent));
        }

        _logger.LogInformation(Logs.Upload.Start);

        HttpRequestMessage request = new(HttpMethod.Post, Routes.UploadApiEndpoint)
        {
            Content = new StringContent(csvContent, System.Text.Encoding.UTF8, "text/csv")
        };
        HttpResponseMessage result = await _httpClient.SendAsync(request);

        if (result.IsSuccessStatusCode)
        {
            _logger.LogInformation(Logs.Upload.Success);
        }
        else
        {
            _logger.LogWarning(Logs.Upload.Failure);
        }

        return result.IsSuccessStatusCode;
    }
}
