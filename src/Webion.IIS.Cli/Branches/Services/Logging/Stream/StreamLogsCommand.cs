using Microsoft.AspNetCore.SignalR.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Core;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications.Logging;

namespace Webion.IIS.Cli.Branches.Services.Logging.Stream;

public sealed class StreamLogsCommand : AsyncCommand<StreamLogsCommandSettings>
{
    private readonly ICliApplicationLifetime _lifetime;

    public StreamLogsCommand(ICliApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, StreamLogsCommandSettings settings)
    {
        var service = await DeploySettings.GetServiceFromFileAsync(
            path: settings.SettingsFile ?? "deploy.yml",
            serviceName: settings.ServiceName,
            cancellationToken: _lifetime.CancellationToken
        );
        
        if (service is null)
        {
            AnsiConsole.MarkupLine(Msg.Err("Service not configured"));
            return 1;
        }

        var env = service.GetEnvironment(settings.Env);
        await using var client = new HubConnectionBuilder()
            .WithUrl(new Uri(env.DaemonAddress, "v1/hubs/applications"))
            .Build();

        await client.StartAsync(_lifetime.CancellationToken);

        var logs = client.StreamAsync<LogDto>("streamLogs", new StreamLogsRequest
        {
            SiteId = env.SiteId,
            AppId = env.AppId,
            LogDirectory = env.LogDir,
        });

        return await AnsiConsole.Status().StartAsync("Streaming logs", async _ =>
        {
            await foreach (var log in logs)
            {
                AnsiConsole.WriteLine(log.Message);
            }

            return 0;
        });
    }
}