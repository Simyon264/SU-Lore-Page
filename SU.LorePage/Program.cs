using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SU.LorePage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Configuration.AddJsonFile("appsettings.json", optional: true);
builder.Configuration.AddJsonFile("appsettings.Secret.json", optional: true);

var app = builder.Build();

await app.RunAsync();