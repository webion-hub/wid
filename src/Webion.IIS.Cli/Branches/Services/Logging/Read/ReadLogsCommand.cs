using System.Text;
using Humanizer;
using Humanizer.Bytes;
using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Core;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Cli.Ui.Errors;
using Webion.IIS.Client;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications.Logging;

namespace Webion.IIS.Cli.Branches.Services.Logging.Read;

public sealed class ReadLogsCommand : AsyncCommand<ReadLogsCommandSettings>
{
    private readonly IIISDaemonClient _iis;
    private readonly ICliApplicationLifetime _lifetime;

    public ReadLogsCommand(ICliApplicationLifetime lifetime, IIISDaemonClient iis)
    {
        _lifetime = lifetime;
        _iis = iis;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, ReadLogsCommandSettings settings)
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
        _iis.BaseAddress = env.DaemonAddress;

        var enumerateLogsResponse = await AnsiConsole.Status().StartAsync("Enumerating files", async _ =>
        {
            return await _iis.Applications.EnumerateLogsAsync(
                siteId: env.SiteId,
                appId: env.AppId,
                logsPath: env.LogDir,
                cancellationToken: _lifetime.CancellationToken
            );
        });
        
        if (!enumerateLogsResponse.IsSuccessStatusCode)
        {
            AnsiConsole.Write(ApiErrorControl.From(enumerateLogsResponse));
            return 1;
        }
            
        var file = AnsiConsole.Prompt(new SelectionPrompt<LogFileDto>()
            .AddChoices(enumerateLogsResponse.Content.LogFiles)
            .UseConverter(f => $"[green]{f.LastWriteTimeUtc}[/] [blue]({ByteSize.FromBytes(f.BytesSize).Humanize()})[/] {f.Name}")
        );
        
        return await AnsiConsole.Status().StartAsync("Fetching log", async _ =>
        {
            var readLogResponse = await _iis.Applications.ReadLogAsync(
                siteId: env.SiteId,
                appId: env.AppId,
                base64FileName: Base64Id.Serialize(Path.Combine(env.LogDir, file.Name)),
                cancellationToken: _lifetime.CancellationToken
            );

            if (!readLogResponse.IsSuccessStatusCode)
            {
                AnsiConsole.Write(ApiErrorControl.From(readLogResponse));
                return 2;
            }

            using var reader = new StreamReader(readLogResponse.Content);
            var rawText = await reader.ReadToEndAsync(_lifetime.CancellationToken);
            
            AnsiConsole.WriteLine(rawText);
            return 0;
        });
    }
}