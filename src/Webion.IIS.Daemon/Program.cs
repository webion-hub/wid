using System.Text.Json.Serialization;
using Webion.AspNetCore;
using Webion.IIS.Daemon.Config;
using Webion.IIS.Daemon.Config.OpenApi;
using Webion.IIS.Daemon.Hubs.v1.Applications;

var builder = WebApplication.CreateBuilder();

builder.Add<CorsConfig>();
builder.Add<VersioningConfig>();
builder.Add<OpenApiConfig>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSignalR();

builder.Services.AddProblemDetails();


var app = builder.Build();

app.Use<CorsConfig>();

app.Use<OpenApiConfig>();

app.MapControllers();
app.MapHub<StreamLogsHub>("/v{version:apiVersion}/hubs/applications");

var port = args.FirstOrDefault() ?? "8080";
var url = $"http://0.0.0.0:{port}";
app.Urls.Add(url);

app.Run();