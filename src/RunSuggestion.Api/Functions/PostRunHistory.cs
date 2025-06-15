using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RunSuggestion.Core.Interfaces;

namespace RunSuggestion.Api.Functions;

public class PostRunHistory
{
    private readonly ILogger<PostRunHistory> _logger;
    private readonly IAuthenticator _authenticator;
    private readonly IRunHistoryAdder _runHistoryAdder;

    public PostRunHistory(ILogger<PostRunHistory> logger, IAuthenticator authenticator, IRunHistoryAdder runHistoryAdder)
    {
        _logger = logger;
        _authenticator = authenticator;
        _runHistoryAdder = runHistoryAdder;
    }

    [Function("PostRunHistory")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest request)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");

    }

}
