using RunSuggestion.Web.Constants;
using RunSuggestion.Web.Interfaces;

namespace RunSuggestion.Web.Services;

public class CsvUploadService(ILogger<CsvUploadService> logger) : ICsvUploadService
{
    private readonly ILogger<CsvUploadService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task<bool> Upload(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
        {
            throw new ArgumentException(Errors.Upload.NoCsvContent, nameof(csvContent));
        }

        _logger.LogInformation(Logs.Upload.Start);

        return Task.FromResult(true);
    }
}
