using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Api.Constants;

namespace RunSuggestion.Api.Functions;

public class GetHealthCheck(ILogger<GetHealthCheck> logger)
{
    private readonly ILogger<GetHealthCheck> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [Function(nameof(GetHealthCheck))]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation(Messages.HealthCheck.RequestReceived);
        return new OkObjectResult(Messages.HealthCheck.Success);
    }
}
