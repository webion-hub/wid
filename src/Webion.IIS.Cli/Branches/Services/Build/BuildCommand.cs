using CliWrap;
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

        return await AnsiConsole.Status().StartAsync("Building", async ctx =>
        {
            foreach (var step in service.Build)
            {
                var commands = step.Run
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    .Where(x => x.Length != 0)
                    .Select(x => new CliCommand(
                        Name: x[0],
                        Args: x[1..]
                    ));
                
                foreach (var cmd in commands)
                {
                    ctx.Status(cmd.ToString());
                    await CliWrap.Cli.Wrap(cmd.Name)
                        .WithArguments(cmd.Args)
                        .WithWorkingDirectory(step.WorkDir)
                        .WithStandardOutputPipe(PipeTarget.ToDelegate(AnsiConsole.WriteLine))
                        .WithStandardErrorPipe(PipeTarget.ToDelegate(AnsiConsole.WriteLine))
                        .ExecuteAsync(_lifetime.CancellationToken);
                    
                    AnsiConsole.MarkupLine(Msg.Ok(cmd.ToString()));
                }
            }
            

            AnsiConsole.MarkupLine(Msg.Ok("Build completed"));
            return 0;
        });
    }
}

file record CliCommand(string Name, string[] Args)
{
    public override string ToString()
    {
        return $"{Name} {string.Join(' ', Args)}";
    }
}