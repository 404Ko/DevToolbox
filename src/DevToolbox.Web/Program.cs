using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DevToolbox.Web;
using DevToolbox.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// AI Accept [Added]: Register tool services
builder.Services.AddScoped<IEncodingService, EncodingService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IFormatterService, FormatterService>();
builder.Services.AddScoped<ITimeService, TimeService>();
builder.Services.AddScoped<IGeneratorService, GeneratorService>();

await builder.Build().RunAsync();
