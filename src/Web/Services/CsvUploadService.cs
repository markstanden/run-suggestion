using RunSuggestion.Web.Interfaces;

namespace RunSuggestion.Web.Services;

public class CsvUploadService(ILogger<CsvUploadService> logger) : ICsvUploadService
{
    private readonly ILogger<CsvUploadService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task<bool> Upload(string csvContent) => throw new NotImplementedException();
}
