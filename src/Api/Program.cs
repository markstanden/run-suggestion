using System.Globalization;
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
using Microsoft.Extensions.Logging;
using RunSuggestion.Shared.Models.Runs;

FunctionsApplicationBuilder builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

string connectionString = builder.Configuration[DatabaseConstants.SqliteConnection.Key] ??
                          DatabaseConstants.SqliteConnection.Default;

builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
builder.Services.AddScoped<IAuthenticator, SwaAuthenticationService>();
builder.Services.AddScoped<ICsvParser, CsvParser>();
builder.Services.AddScoped<IRunHistoryTransformer, CsvToRunHistoryTransformer>();
builder.Services.AddScoped<IRunHistoryAdder, TrainingPeaksHistoryService>();
builder.Services.AddScoped<IRecommendationService>(serviceProvider =>
{
    ILogger<RecommendationService> logger = serviceProvider.GetRequiredService<ILogger<RecommendationService>>();
    IUserRepository userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    string configDateOverride = builder.Configuration["TestCurrentDate"] ?? string.Empty;

    return DateTime.TryParseExact(configDateOverride,
                                  "yyyy-MM-dd",
                                  CultureInfo.InvariantCulture,
                                  DateTimeStyles.RoundtripKind,
                                  out DateTime parsedDate)
        ? new RecommendationService(logger, userRepository, parsedDate)
        : new RecommendationService(logger, userRepository);
});

builder.Services.AddScoped<IValidator<RunEvent>>(_ => new RunEventValidator(DateTime.UtcNow));

await builder
    .Build()
    .RunAsync();
