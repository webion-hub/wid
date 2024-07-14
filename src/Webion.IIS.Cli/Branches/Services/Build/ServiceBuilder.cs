using CliWrap;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Webion.IIS.Cli.Core;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;

namespace Webion.IIS.Cli.Branches.Services.Build;

public sealed class ServiceBuilder
{
    private readonly ICliApplicationLifetime _lifetime;
    private readonly ILogger<ServiceBuilder> _logger;

    public ServiceBuilder(ICliApplicationLifetime lifetime, ILogger<ServiceBuilder> logger)
    {
        _lifetime = lifetime;
        _logger = logger;
    }

    public async Task<int> BuildAsync(ServiceSettings service, EnvironmentSettings env)
    {
        return await AnsiConsole.Status().StartAsync("Building", async ctx =>
        {
            ctx.Status("Loading env vars");
            var envVars = await env.LoadEnvVarsAsync(_lifetime.CancellationToken);
            
            if (env.EnvFile is not null)
                AnsiConsole.MarkupLine(Msg.Ok($"Loaded env vars from file {env.EnvFile}"));
            
            _logger.LogInformation("Loaded env vars from file {File}: {@Vars}",
                env.EnvFile,
                envVars.Select(x => $"{x.Key}={x.Value}")
            );
            
            foreach (var step in service.Build)
            {
                var commands = step.Run
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(x => x.Split(" ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    .Where(x => x.Length != 0)
                    .Select(x => new CliCommand(
                        Name: x[0],
                        Args: x[1..],
                        EnsureNonZero: step.EnsureNonZero
                    ))
                    .ToList();

                var i = 1;
                foreach (var cmd in commands)
                {
                    ctx.Status($"[b]{step.Name} ({i++}/{commands.Count})[/] {cmd}");
                    var result = await CliWrap.Cli.Wrap("bash")
                        .WithArguments(["-c", cmd.ToString()])
                        .WithWorkingDirectory(step.WorkDir)
                        .WithEnvironmentVariables(envVars)
                        .WithStandardOutputPipe(PipeTarget.ToDelegate(AnsiConsole.WriteLine))
                        .WithStandardErrorPipe(PipeTarget.ToDelegate(AnsiConsole.WriteLine))
                        .WithValidation(cmd.EnsureNonZero
                            ? CommandResultValidation.ZeroExitCode
                            : CommandResultValidation.None
                        )
                        .ExecuteAsync(_lifetime.CancellationToken);

                    var runTime = result.RunTime;
                    var runTimeFmt = $"{runTime.Hours:00}:{runTime.Minutes:00}:{runTime.Seconds:00}";
                    var icon = result.IsSuccess
                        ? Icons.Ok
                        : Icons.Err;
                    
                    AnsiConsole.MarkupLine(
                        $"{icon} [blue]{result.ExitTime:HH:mm:ss}[/] [b]{cmd}[/] [gray]took {runTimeFmt}[/]"
                    );
                }
            }
            
            AnsiConsole.MarkupLine(Msg.Ok("Build completed"));
            return 0;
        });
    }
}

file record CliCommand(string Name, string[] Args, bool EnsureNonZero)
{
    public override string ToString()
    {
        return $"{Name} {string.Join(' ', Args)}";
    }
}