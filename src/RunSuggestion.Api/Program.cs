using RunSuggestion.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RunSuggestion.Core.Constants;
using RunSuggestion.Core.Models.Runs;
using RunSuggestion.Core.Repositories;
using RunSuggestion.Core.Services;
using RunSuggestion.Core.Transformers;
using RunSuggestion.Core.Validators;

IHost host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        string connectionString = context.Configuration[DatabaseConstants.SqliteConnection.Key] ??
                                  DatabaseConstants.SqliteConnection.Default;
        services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
        services.AddScoped<IAuthenticator, AuthenticationService>();
        services.AddScoped<ICsvParser, CsvParser>();
        services.AddScoped<IRunHistoryTransformer, CsvToRunHistoryTransformer>();
        services.AddScoped<IRunHistoryAdder, TrainingPeaksHistoryService>();
        services.AddScoped<IValidator<RunEvent>>(_ => new RunEventValidator(DateTime.Now));
    })
    .Build();

host.Run();
