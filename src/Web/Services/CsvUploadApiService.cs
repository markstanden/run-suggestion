using System.Net.Http.Json;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Shared.Models.Dto;
using RunSuggestion.Web.Constants;
using RunSuggestion.Web.Interfaces;

namespace RunSuggestion.Web.Services;

public class CsvUploadApiService(ILogger<CsvUploadApiService> logger, HttpClient httpClient) : ICsvUploadApiService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<CsvUploadApiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<int> UploadAsync(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            throw new ArgumentException(Errors.History.NoCsvContent, nameof(csvContent));
        }

        _logger.LogInformation(Logs.Upload.Start);

        HttpRequestMessage request = new(HttpMethod.Post, Routes.UploadPath)
        {
            Content = new StringContent(csvContent, System.Text.Encoding.UTF8, "text/csv")
        };
        HttpResponseMessage response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation(Logs.Upload.Success);
            UploadResponse? uploadResponse = await response.Content.ReadFromJsonAsync<UploadResponse>();
            return uploadResponse?.RowsAdded ?? 0;
        }
        else
        {
            _logger.LogWarning(Logs.Upload.Failure);
            return 0;
        }
    }
}
