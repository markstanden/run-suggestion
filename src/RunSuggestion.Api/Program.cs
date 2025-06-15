using RunSuggestion.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RunSuggestion.Core.Services;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddScoped<IAuthenticator, AuthenticationService>();
        services.AddScoped<IRunHistoryAdder, RunHistoryService>(service => new RunHistoryService());
    })
    .Build();

host.Run();
