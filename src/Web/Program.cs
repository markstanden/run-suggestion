using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RunSuggestion.Shared.Constants;
using RunSuggestion.Web;
using RunSuggestion.Web.Authentication;
using RunSuggestion.Web.Interfaces;
using RunSuggestion.Web.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

IConfigurationSection loggingConfigSection = builder.Configuration.GetSection("Logging");
if (loggingConfigSection.Exists())
{
    builder.Logging.AddConfiguration(loggingConfigSection);
}

///////////////////////////
// Blazor initialisation
///////////////////////////
// Adds Blazor App class to the html element with the id `app`
builder.RootComponents.Add<App>("#app");
// This adds the HeadOutlet class to the end of <head>
// HeadOutlet contains dynamic page information, like <PageTitle>
builder.RootComponents.Add<HeadOutlet>("head::after");

UriBuilder uriBuilder = new(builder.HostEnvironment.BaseAddress);
uriBuilder.Path = Routes.ApiBasePath;
builder.Services
    .AddScoped(_ => new HttpClient { BaseAddress = uriBuilder.Uri });

// Register Azure StaticWebApp specific authentication
builder.Services.AddScoped<AuthenticationStateProvider, StaticWebAppsAuthStateProvider>();
builder.Services.AddScoped<ICsvUploadApiService, CsvUploadApiService>();
builder.Services.AddScoped<IRecommendationApiService, RecommendationApiService>();
builder.Services.AddAuthorizationCore();
await builder.Build().RunAsync();
