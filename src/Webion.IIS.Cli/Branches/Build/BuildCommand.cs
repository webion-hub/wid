using CliWrap;
using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;

namespace Webion.IIS.Cli.Branches.Build;

public sealed class BuildCommand : AsyncCommand<BuildCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, BuildCommandSettings settings)
    {
        var deploySettings = await DeploySettings.TryReadFromFileAsync(settings.SettingsFile ?? "deploy.yml");
        if (!deploySettings.Services.TryGetValue(settings.ServiceName, out var service))
        {
            AnsiConsole.MarkupLine(Msg.Err("Service not configured"));
            return 1;
        }

        return await AnsiConsole.Status().StartAsync("Build service", async ctx =>
        {
            var command = service.BuildCommand.Split(" ");

            await CliWrap.Cli.Wrap(command.First())
                .WithArguments(command.Skip(1))
                .WithStandardOutputPipe(PipeTarget.ToDelegate((str) => AnsiConsole.MarkupLine(str!)))
                .ExecuteAsync();

            AnsiConsole.MarkupLine($"{Icons.Ok} Service builded");
            return 0;
        });
    }
}