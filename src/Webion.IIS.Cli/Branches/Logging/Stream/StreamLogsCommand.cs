using Microsoft.AspNetCore.SignalR.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Core;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications;

namespace Webion.IIS.Cli.Branches.Logging.Stream;

public sealed class StreamLogsCommand : AsyncCommand<StreamLogsCommandSettings>
{
    private readonly ICliApplicationLifetime _lifetime;

    public StreamLogsCommand(ICliApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, StreamLogsCommandSettings settings)
    {
        var deploySettings = await DeploySettings.TryReadFromFileAsync(settings.SettingsFile ?? "deploy.yml");
        if (!deploySettings.Services.TryGetValue(settings.ServiceName, out var service))
        {
            AnsiConsole.MarkupLine(Msg.Err("Service not configured"));
            return 1;
        }

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                return true;
            }
        };

        await using var client = new HubConnectionBuilder()
            .WithUrl(new Uri(service.DaemonAddress, "v1/hubs/applications"),
            options =>
            {
                options.HttpMessageHandlerFactory = options.HttpMessageHandlerFactory = _ => handler;
            })
            .Build();

        await client.StartAsync(_lifetime.CancellationToken);

        var logs = client.StreamAsync<LogDto>("streamLogs", new StreamLogsRequest
        {
            SiteId = service.SiteId,
            AppId = Base64Id.Serialize(service.AppPath),
            LogDirectory = service.LogDir.StartsWith('\\')
                ? service.LogDir
                : '\\' + service.LogDir
        });

        return await AnsiConsole.Status().StartAsync("Streaming", async _ =>
        {
            await foreach (var log in logs)
            {
                AnsiConsole.WriteLine(log.Message);
            }

            return 0;
        });
    }
}