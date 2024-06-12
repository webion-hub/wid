using System.Text.Json.Serialization;
using Webion.IIS.Daemon.Hubs.v1.Applications;

var builder = WebApplication.CreateBuilder();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddSignalR();

builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapHub<StreamLogsHub>("/v1/hubs/applications");

var port = args.FirstOrDefault() ?? "5000";
var url = $"http://0.0.0.0:{port}";
app.Urls.Add(url);

app.Run();