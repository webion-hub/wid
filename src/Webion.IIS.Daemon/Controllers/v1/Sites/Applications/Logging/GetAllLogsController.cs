using Microsoft.AspNetCore.Mvc;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications.Logging;
using Webion.IIS.Daemon.Managers;

namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications.Logging;

[ApiController]
[Route("v{version:apiVersion}/sites/{siteId:long}/applications/{appId}/logs")]
public sealed class GetAllLogsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll(
        [FromRoute] long siteId,
        [FromRoute] string appId,
        [FromQuery] string logsPath
    )
    {
        using var manager = new IISManager();
        var findAppResult = manager.FindApp(siteId, appId);

        if (findAppResult is FindAppResult.SiteNotFound)
            ModelState.AddModelError(nameof(siteId), "Site not found");

        if (findAppResult is FindAppResult.AppNotFound)
            ModelState.AddModelError(nameof(siteId), "App not found");

        if (!ModelState.IsValid)
            return ValidationProblem();

        var (_, app) = (FindAppResult.Found)findAppResult;

        var baseFolder = app.VirtualDirectories["/"].PhysicalPath;
        var logsFolder = Path.Combine(baseFolder, logsPath);
        var logFiles = Directory
            .EnumerateFiles(logsFolder)
            .Select(x => new FileInfo(x))
            .OrderByDescending(x => x.LastWriteTimeUtc)
            .Select(x => new LogFileDto
            {
                Name = x.Name,
                BytesSize = x.Length,
                LastWriteTimeUtc = x.LastWriteTimeUtc,
            });

        return Ok(new GetAllLogsResponse
        {
            LogFiles = logFiles,
        });
    }
}