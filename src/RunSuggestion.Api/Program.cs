using RunSuggestion.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RunSuggestion.Core.Services;
using RunSuggestion.Core.Transformers;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddScoped<IAuthenticator, AuthenticationService>();
        services.AddScoped<ICsvParser, CsvParser>();
        services.AddScoped<IRunHistoryTransformer, CsvToRunHistoryTransformer>();
        services.AddScoped<IRunHistoryAdder, TrainingPeaksHistoryService>();
    })
    .Build();

host.Run();
