using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Cli.Ui.Errors;
using Webion.IIS.Client;
using Webion.IIS.Core.ValueObjects;

namespace Webion.IIS.Cli.Branches.Services.Stop;

public sealed class StopServiceCommand : AsyncCommand<StopServiceCommandSettings>
{
    private readonly IIISDaemonClient _iis;

    public StopServiceCommand(IIISDaemonClient iis)
    {
        _iis = iis;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, StopServiceCommandSettings settings)
    {
        var service = await DeploySettings.GetServiceFromFileAsync(
            path: settings.SettingsFile ?? "deploy.yml",
            serviceName: settings.ServiceName,
            cancellationToken: default
        );
        
        if (service is null)
        {
            AnsiConsole.MarkupLine(Msg.Err("Service not configured"));
            return 1;
        }

        var env = service.GetEnvironment(settings.Env);
        _iis.BaseAddress = env.DaemonAddress;

        return await AnsiConsole.Status().StartAsync("Stopping service", async ctx =>
        {
            var appId = Base64Id.Serialize(env.AppPath);

            ctx.Status("Stopping service");
            var stopResponse = await _iis.Applications.StopAsync(env.SiteId, appId);
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