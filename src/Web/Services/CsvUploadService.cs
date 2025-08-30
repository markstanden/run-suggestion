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

        HttpRequestMessage request = new(HttpMethod.Post, csvContent);
        HttpResponseMessage result = await _httpClient.SendAsync(request);

        return result.IsSuccessStatusCode;
    }
}
