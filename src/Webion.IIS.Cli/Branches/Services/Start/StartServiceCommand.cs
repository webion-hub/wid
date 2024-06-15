using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Cli.Ui.Errors;
using Webion.IIS.Client;
using Webion.IIS.Core.ValueObjects;

namespace Webion.IIS.Cli.Branches.Services.Start;

public sealed class StartServiceCommand : AsyncCommand<StartServiceCommandSettings>
{
    private readonly IIISDaemonClient _iis;

    public StartServiceCommand(IIISDaemonClient iis)
    {
        _iis = iis;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, StartServiceCommandSettings settings)
    {
        var deploySettings = await DeploySettings.TryReadFromFileAsync(settings.SettingsFile ?? "deploy.yml");
        if (!deploySettings.Services.TryGetValue(settings.ServiceName, out var service))
        {
            AnsiConsole.MarkupLine(Msg.Err("Service not configured"));
            return 1;
        }

        _iis.BaseAddress = service.DaemonAddress;

        return await AnsiConsole.Status().StartAsync("Starting service", async ctx =>
        {
            var appId = Base64Id.Serialize(service.AppPath);

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