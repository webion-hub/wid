using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Cli.Ui.Errors;
using Webion.IIS.Client;
using Webion.IIS.Core.ValueObjects;

namespace Webion.IIS.Cli.Branches.Service.Stop;

public sealed class StopServiceCommand : AsyncCommand<StopServiceCommandSettings>
{
    private readonly IIISDaemonClient _iis;

    public StopServiceCommand(IIISDaemonClient iis)
    {
        _iis = iis;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, StopServiceCommandSettings settings)
    {
        var deploySettings = await DeploySettings.TryReadFromFileAsync(settings.SettingsFile ?? "deploy.yml");
        if (!deploySettings.Services.TryGetValue(settings.ServiceName, out var service))
        {
            AnsiConsole.MarkupLine(Msg.Err("Service not configured"));
            return 1;
        }

        _iis.BaseAddress = service.DaemonAddress;

        return await AnsiConsole.Status().StartAsync("Stopping service", async ctx =>
        {
            var appId = Base64Id.Serialize(service.AppPath);

            ctx.Status("Stopping service");
            var stopResponse = await _iis.Applications.StopAsync(service.SiteId, appId);
            if (!stopResponse.IsSuccessStatusCode)
            {
                AnsiConsole.Write(ApiErrorTable.From(stopResponse));
                return -1;
            }

            AnsiConsole.MarkupLine($"{Icons.Ok} Service stopped");
            return 0;
        });
    }
}