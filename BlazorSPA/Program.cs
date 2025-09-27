using BlazorSPA;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    //options.ProviderOptions.DefaultAccessTokenScopes.Add("api://6a7ceaa2-8d6f-40aa-8f90-fe5268031244/access_as_user");
    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://a1538952-ee4b-42df-8bf7-2fb91988382f/user_impersonation");
});

await builder.Build().RunAsync();
