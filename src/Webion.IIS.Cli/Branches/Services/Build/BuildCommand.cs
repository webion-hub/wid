using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Core;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;

namespace Webion.IIS.Cli.Branches.Services.Build;

public sealed class BuildCommand : AsyncCommand<BuildCommandSettings>
{
    private readonly ICliApplicationLifetime _lifetime;

    public BuildCommand(ICliApplicationLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, BuildCommandSettings settings)
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

        var builder = new ServiceBuilder(_lifetime);
        return await builder.BuildAsync(service);
    }
}