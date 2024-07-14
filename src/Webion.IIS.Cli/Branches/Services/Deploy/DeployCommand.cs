using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Refit;
using Spectre.Console;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Branches.Services.Build;
using Webion.IIS.Cli.Core;
using Webion.IIS.Cli.Helpers.Progress;
using Webion.IIS.Cli.Settings;
using Webion.IIS.Cli.Ui;
using Webion.IIS.Cli.Ui.Errors;
using Webion.IIS.Client;

namespace Webion.IIS.Cli.Branches.Services.Deploy;

public sealed class DeployCommand : AsyncCommand<DeployCommandSettings>
{
    private readonly IIISDaemonClient _iis;
    private readonly ICliApplicationLifetime _lifetime;
    private readonly ILogger<DeployCommand> _logger;
    private readonly ServiceBuilder _serviceBuilder;

    public DeployCommand(IIISDaemonClient iis, ICliApplicationLifetime lifetime, ILogger<DeployCommand> logger, ServiceBuilder serviceBuilder)
    {
        _iis = iis;
        _lifetime = lifetime;
        _logger = logger;
        _serviceBuilder = serviceBuilder;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DeployCommandSettings settings)
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

        if (settings.Build)
        {
            var buildResult = await _serviceBuilder.BuildAsync(service, env);
            if (buildResult != 0)
                return buildResult;

            AnsiConsole.WriteLine();
        }
        
        if (!Directory.Exists(service.BundleDir))
        {
            AnsiConsole.MarkupLine(Msg.Err($"Bundle directory {service.BundleDir} does not exist"));
            return -1;
        }
        
        if (!Directory.EnumerateFileSystemEntries(service.BundleDir).Any())
        {
            AnsiConsole.MarkupLine(Msg.Err($"Bundle directory {service.BundleDir} is empty"));
            return -1;
        }
        
        _iis.BaseAddress = env.DaemonAddress;

        var stopResult = await AnsiConsole.Status().StartAsync("Stopping service", 
            async _ => await StopAsync(env)
        );
        
        if (stopResult != 0)
            return stopResult;

        var uploadResult = await AnsiConsole.Progress()
            .Columns([
                new SpinnerColumn(),
                new TaskDescriptionColumn(),
                new DownloadedColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
                new TransferSpeedColumn(),
            ])
            .StartAsync(async ctx => await UploadAsync(settings, service, ctx, env));
        
        if (uploadResult != 0)
            return uploadResult;

        return await AnsiConsole.Status().StartAsync("Starting service",
            async _ => await StartAsync(env)
        );
    }


    private async Task<int> StopAsync(EnvironmentSettings env)
    {
        var stopResponse = await _iis.Applications.StopAsync(env.SiteId, env.AppId, _lifetime.CancellationToken);
        if (!stopResponse.IsSuccessStatusCode)
        {
            AnsiConsole.Write(ApiErrorControl.From(stopResponse));
            return -1;
        }

        AnsiConsole.MarkupLine(Msg.Ok("Service stopped"));
        return 0;
    }
    
    private async Task<int> StartAsync(EnvironmentSettings env)
    {
        var startResponse = await _iis.Applications.StartAsync(env.SiteId, env.AppId, _lifetime.CancellationToken);
        if (!startResponse.IsSuccessStatusCode)
        {
            AnsiConsole.Write(ApiErrorControl.From(startResponse));
            return -1;
        }

        AnsiConsole.MarkupLine(Msg.Ok("Service started"));
        return 0;
    }
    
    private async Task<int> UploadAsync(
        DeployCommandSettings settings,
        ServiceSettings service,
        ProgressContext ctx,
        EnvironmentSettings env
    )
    {
        var tmpDir = await PrepareDeployDirectoryAsync(service, settings);

        await using var stream = new MemoryStream();
        ZipFile.CreateFromDirectory(tmpDir, stream);
        stream.Position = 0;

        var uploadTask = ctx.AddTask("Uploading bundle", maxValue: stream.Length);
        var unzipTask = ctx
            .AddTask("Unzipping files", autoStart: false, maxValue: stream.Length)
            .IsIndeterminate();
        
        await using var progress = new ProgressStream(stream);
        using var _ = progress.OnRead.Subscribe(e =>
        {
            uploadTask.Increment(e.BytesRead);
            if (uploadTask.IsFinished)
            {
                uploadTask.StopTask();
                unzipTask.StartTask();
            }
        });
        
        var deployResponse =  await _iis.Applications.DeployAsync(
            siteId: env.SiteId,
            appId: env.AppId,
            forceDelete: settings.ForceDelete,
            bundle: new StreamPart(progress, "bundle.zip"),
            cancellationToken: _lifetime.CancellationToken
        );
        
        unzipTask.StopTask();
        
        if (!deployResponse.IsSuccessStatusCode)
        {
            AnsiConsole.Write(ApiErrorControl.From(deployResponse));
            return -1;
        }
        
        return 0;
    }

    private async Task<string> PrepareDeployDirectoryAsync(ServiceSettings service, DeployCommandSettings settings)
    {
        var ignores = await GetIgnoresAsync(service, settings);
        
        var tmpDir = Directory.CreateTempSubdirectory("wid_deploy_");
        var files = Directory.EnumerateFiles(service.BundleDir, "*.*", SearchOption.AllDirectories);
        
        _logger.LogDebug("Created temporary directory {TmpDir}", tmpDir);
        _logger.LogDebug("Ignoring files: {Ignores}", string.Join(", ", ignores));
        
        foreach (var file in files)
        {
            _logger.LogTrace("Handling file {File}", file);
            
            var isIgnored = ignores.Any(x => x.IsMatch(file));
            if (isIgnored)
            {
                _logger.LogTrace("File {File} ignored", file);
                continue;
            }

            var tmpFilePath = Path.Combine(tmpDir.FullName, file);
            var tmpFileDir = Path.GetDirectoryName(tmpFilePath);
            if (tmpFileDir is not null)
                Directory.CreateDirectory(tmpFileDir);
            
            File.Copy(file, tmpFilePath);
            _logger.LogTrace("Copied file {File} to {TmpFilePath}", file, tmpFilePath);
        }

        return Path.Combine(tmpDir.FullName, service.BundleDir);
    }

    private async Task<List<Regex>> GetIgnoresAsync(ServiceSettings service, DeployCommandSettings settings)
    {
        if (settings.NoIgnore)
        {
            AnsiConsole.MarkupLine(Msg.Line("Skipping ignore file"));
            return [];
        }
        
        _logger.LogDebug("Searching ignore file {IgnoreFile}", service.IgnoreFile);
        if (!File.Exists(service.IgnoreFile))
        {
            _logger.LogDebug("Ignore file not found");
            return [];
        }

        AnsiConsole.MarkupLine(Msg.Ok("Ignore file found"));
        
        var ignoreFile = await File.ReadAllTextAsync(service.IgnoreFile, _lifetime.CancellationToken);
        return ignoreFile
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new Regex(x))
            .ToList();
    }
}