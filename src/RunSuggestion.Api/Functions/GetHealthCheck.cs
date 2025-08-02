using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RunSuggestion.Api.Functions;

public class GetHealthCheck
{
    public const string RequestReceivedLog = "HealthCheck Endpoint recieved request";
    public const string HealthCheckResponse = "System Healthy";

    private readonly ILogger<GetHealthCheck> _logger;

    public GetHealthCheck(ILogger<GetHealthCheck> logger)
    {
        _logger = logger;
    }

    [Function("HealthCheck")]
    public IActionResult HealthCheck([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation(RequestReceivedLog);
        return new OkObjectResult(HealthCheckResponse);
    }
}
