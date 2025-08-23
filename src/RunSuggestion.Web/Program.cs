using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RunSuggestion.Web;

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

///////////////////////////
// Register Authentication
///////////////////////////
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C",
                               options.ProviderOptions.Authentication);
});

await builder.Build().RunAsync();
