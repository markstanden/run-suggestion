using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RunSuggestion.Web;
using RunSuggestion.Web.Authentication;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

///////////////////////////
// Blazor initialisation
///////////////////////////
// Adds Blazor App class to the html element with the id `app`
builder.RootComponents.Add<App>("#app");
// This adds the HeadOutlet class to the end of <head>
// HeadOutlet contains dynamic page information, like <PageTitle>
builder.RootComponents.Add<HeadOutlet>("head::after");

//////////////////////////
// Register the API
//////////////////////////
builder.Services
    .AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register Azure StaticWebApp specific authentication
builder.Services.AddScoped<AuthenticationStateProvider, StaticWebAppsAuthStateProvider>();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
