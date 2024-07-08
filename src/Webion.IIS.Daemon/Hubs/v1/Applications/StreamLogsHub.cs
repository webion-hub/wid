using Microsoft.AspNetCore.SignalR;
using Microsoft.Web.Administration;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications.Logging;

namespace Webion.IIS.Daemon.Hubs.v1.Applications;

public sealed class StreamLogsHub : Hub
{
    [HubMethodName("streamLogs")]
    public async IAsyncEnumerable<LogDto> StreamLogs(StreamLogsRequest request)
    {
        using var iis = new ServerManager();

        var appPath = Base64Id.Deserialize(request.AppId);
        var app = iis.Sites
            .Where(x => x.Id == request.SiteId)
            .SelectMany(x => x.Applications)
            .Where(x => x.Path == appPath)
            .FirstOrDefault();

        if (app is null)
            yield break;

        var dir = app.VirtualDirectories["/"].PhysicalPath + request.LogDirectory;
        var files = Directory.EnumerateFiles(dir, "*.log");
        var lastLogFile = files
            .Select(x => new FileInfo(x))
            .MaxBy(x => x.LastWriteTimeUtc);

        if (lastLogFile is null)
            yield break;

        await using var stream = new FileStream(lastLogFile.FullName, FileMode.Open, FileAccess.Read);
        using var reader = new StreamReader(stream);
        using var throttle = new PeriodicTimer(TimeSpan.FromMilliseconds(100));

        while (!Context.ConnectionAborted.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(Context.ConnectionAborted);
            if (line is null)
            {
                await throttle.WaitForNextTickAsync(Context.ConnectionAborted);
                continue;
            }

            yield return new LogDto
            {
                Message = line,
            };
        }
    }
}