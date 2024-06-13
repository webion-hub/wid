using MudBlazor.Services;
using Webion.IIS.Client;
using Webion.IIS.UI.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services
    .AddHttpClient<IIISDaemonClient, IISDaemonClient>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["ApiUrl"] ?? "http://localhost:5000");
    });

var app = builder.Build();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Urls.Add("http://localhost:5000");

app.Run();