using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Webion.IIS.Client;
using Webion.IIS.UI;

var builder = WebAssemblyHostBuilder.CreateDefault();
builder.RootComponents.Add<App>("#app");

builder.Services.AddMudServices();

builder.Services
    .AddHttpClient<IIISDaemonClient, IISDaemonClient>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri("http://192.168.1.194:8080");
    });

await builder.Build().RunAsync();