using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Webion.IIS.Daemon.Managers;

namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications.Archive;

[ApiController]
[Route("v{version:apiVersion}/sites/{siteId:long}/applications/{appId}/backups")]
[ApiVersion("1.0")]
[Tags("Applications")]
public sealed class GetAllBackupsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<GetAllBackupsResponse>(200)]
    public IActionResult GetAll(
        [FromRoute] long siteId,
        [FromRoute] string appId
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
        
        var backupFolder = Path.Combine("~/.wid/backups", app.Path);
        if (!Directory.Exists(backupFolder))
            Directory.CreateDirectory(backupFolder);

        var files = Directory
            .EnumerateFiles(backupFolder, "*.zip", SearchOption.TopDirectoryOnly)
            .Select(x => new FileInfo(x))
            .Select(x => new BackupFolderDto
            {
                Name = x.Name,
                BytesSize = x.Length,
                CreatedAtUtc = x.CreationTimeUtc,
            });
        
        return Ok(new GetAllBackupsResponse
        {
            Backups = files
        });
    }
}