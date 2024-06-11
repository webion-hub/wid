using System.IO.Compression;
using Refit;
using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Cli.Ui.Errors;
using Webion.IIS.Client;
using Webion.IIS.Core.ValueObjects;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Webion.IIS.Cli.Branches.Deploy;

public sealed class DeployCommand : AsyncCommand<DeployCommandSettings>
{
    private readonly IIISDaemonClient _iis;

    public DeployCommand(IIISDaemonClient iis)
    {
        _iis = iis;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DeployCommandSettings settings)
    {
        var deploySettings = await DeploySettings.TryReadFromFileAsync(settings.SettingsFile ?? "deploy.yml");
        if (!deploySettings.Services.TryGetValue(settings.ServiceName, out var service))
        {
            AnsiConsole.MarkupLine(Msg.Err("Service not configured"));
            return 1;
        }

        if (!Directory.Exists(service.BundleDir))
        {
            AnsiConsole.MarkupLine(Msg.Err($"Could not find directory {service.BundleDir}"));
            return -1;
        }

        if (service.IsProduction)
        {
            var commit = AnsiConsole.Confirm(
                prompt: Msg.Ask("Deploy to production"),
                defaultValue: false
            );
            
            if (!commit)
                return 0;
        }

        _iis.BaseAddress = service.DaemonAddress;

        return await AnsiConsole.Status().StartAsync("Stopping service", async ctx =>
        {
            var appId = Base64Id.Serialize(service.AppPath);
            
            var stopResponse = await _iis.Applications.StopAsync(service.SiteId, appId);
            if (!stopResponse.IsSuccessStatusCode)
            {
                AnsiConsole.Write(ApiErrorTable.From(stopResponse));
                return -1;
            }
            
            AnsiConsole.MarkupLine($"{Icons.Ok} Service stopped");

            ctx.Status("Uploading bundle");
            using var stream = new MemoryStream();
            ZipFile.CreateFromDirectory(service.BundleDir, stream);
            stream.Position = 0;

            var deployResponse = await _iis.Applications.DeployAsync(
                siteId: service.SiteId,
                appId: appId,
                bundle: new StreamPart(stream, "bundle.zip"),
                cancellationToken: CancellationToken.None
            );

            if (!deployResponse.IsSuccessStatusCode)
            {
                AnsiConsole.Write(ApiErrorTable.From(deployResponse));
                return -1;
            }
            
            AnsiConsole.MarkupLine($"{Icons.Ok} Bundle uploaded");

            ctx.Status("Starting service");
            var startResponse = await _iis.Applications.StartAsync(service.SiteId, appId);
            if (!startResponse.IsSuccessStatusCode)
            {
                AnsiConsole.Write(ApiErrorTable.From(startResponse));
                return -1;
            }
            
            
            AnsiConsole.MarkupLine($"{Icons.Ok} Service started");
            return 0;
        });
    }
}