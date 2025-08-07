using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RunSuggestion.Api.Functions;

public class GetRunSuggestion
{
    private readonly ILogger<GetRunSuggestion> _logger;

    public GetRunSuggestion(ILogger<GetRunSuggestion> logger)
    {
        _logger = logger;
    }

    [Function("GetRunSuggestion")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        _logger.LogInformation(string.Empty);
        return new OkObjectResult(string.Empty);
    }
}
