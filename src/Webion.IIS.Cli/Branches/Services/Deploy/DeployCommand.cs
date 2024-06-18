using System.IO.Compression;
using Refit;
using Spectre.Console;
using Spectre.Console.Cli;
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

    public DeployCommand(IIISDaemonClient iis, ICliApplicationLifetime lifetime)
    {
        _iis = iis;
        _lifetime = lifetime;
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

        if (!Directory.Exists(service.BundleDir))
        {
            AnsiConsole.MarkupLine(Msg.Err($"Could not find directory {service.BundleDir}"));
            return -1;
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

        AnsiConsole.MarkupLine($"{Icons.Ok} Service started");
        return 0;
    }
    
    private async Task<int> UploadAsync(
        DeployCommandSettings settings,
        ServiceSettings service,
        ProgressContext ctx,
        EnvironmentSettings env
    )
    {
        await using var stream = new MemoryStream();
        var tmpDir = Directory.CreateTempSubdirectory("wid_deploy");
        var files = Directory.GetFiles(service.BundleDir);
        foreach (var file in files)
            File.Copy(file, Path.Combine(tmpDir.FullName, file));
        
        ZipFile.CreateFromDirectory(tmpDir.FullName, stream);
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
}