using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Managers;

namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications.Logging;

[ApiController]
[Route("v{version:apiVersion}/sites/{siteId:long}/applications/{appId}/logs/{base64LogFile}")]
public sealed class GetLogController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromRoute] long siteId,
        [FromRoute] string appId,
        [FromRoute, Base64String] string base64LogFile,
        CancellationToken cancellationToken
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
        var logFile = Base64Id.Deserialize(base64LogFile);
        var logPath = Path.Combine(baseFolder, logFile);

        if (!System.IO.File.Exists(logPath))
        {
            return Problem(
                detail: "File not found",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        var text = await System.IO.File.ReadAllBytesAsync(logPath, cancellationToken);
        return File(
            fileContents: text,
            contentType: "text/plain"
        );
    }
}