using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Core;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Cli.Ui.Errors;
using Webion.IIS.Client;

namespace Webion.IIS.Cli.Branches.Services.Recycle;

public sealed class RecycleServiceCommand : AsyncCommand<RecycleServiceCommandSettings>
{
    private readonly IIISDaemonClient _iis;
    private readonly ICliApplicationLifetime _lifetime;

    public RecycleServiceCommand(IIISDaemonClient iis, ICliApplicationLifetime lifetime)
    {
        _iis = iis;
        _lifetime = lifetime;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, RecycleServiceCommandSettings settings)
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
        if (env.IsProduction)
        {
            var commit = AnsiConsole.Confirm(
                prompt: Msg.Ask("Deploy to production"),
                defaultValue: false
            );

            if (!commit)
                return 0;
        }
        _iis.BaseAddress = env.DaemonAddress;

        return await AnsiConsole.Status().StartAsync("Stopping service", async ctx =>
        {
            var stopResponse = await _iis.Applications.StopAsync(env.SiteId, env.AppId, _lifetime.CancellationToken);
            if (!stopResponse.IsSuccessStatusCode)
            {
                AnsiConsole.Write(ApiErrorControl.From(stopResponse));
                return 1;
            }
            
            AnsiConsole.MarkupLine(Msg.Ok("Service stopped"));

            ctx.Status("Starting service");
            var startResponse = await _iis.Applications.StartAsync(env.SiteId, env.AppId, _lifetime.CancellationToken);
            if (!startResponse.IsSuccessStatusCode)
            {
                AnsiConsole.Write(ApiErrorControl.From(startResponse));
                return 2;
            }
            
            AnsiConsole.MarkupLine(Msg.Ok("Service started"));
            return 0;
        });
    }
}