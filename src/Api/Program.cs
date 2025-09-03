using RunSuggestion.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RunSuggestion.Core.Constants;
using RunSuggestion.Core.Repositories;
using RunSuggestion.Core.Services;
using RunSuggestion.Core.Transformers;
using RunSuggestion.Core.Validators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using RunSuggestion.Shared.Models.Runs;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

string connectionString = builder.Configuration[DatabaseConstants.SqliteConnection.Key] ??
                          DatabaseConstants.SqliteConnection.Default;

builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
builder.Services.AddScoped<IAuthenticator, JwtAuthenticationService>();
builder.Services.AddScoped<ICsvParser, CsvParser>();
builder.Services.AddScoped<IRunHistoryTransformer, CsvToRunHistoryTransformer>();
builder.Services.AddScoped<IRunHistoryAdder, TrainingPeaksHistoryService>();
builder.Services.AddScoped<IValidator<RunEvent>>(_ => new RunEventValidator(DateTime.Now));

await builder
    .Build()
    .RunAsync();
