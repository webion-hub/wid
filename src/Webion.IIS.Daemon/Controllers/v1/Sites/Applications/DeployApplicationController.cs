using System.IO.Compression;
using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Webion.IIS.Daemon.Managers;

namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications;

[ApiController]
[Route("v{version:apiVersion}/sites/{siteId:long}/applications/{appId}/deploy")]
[ApiVersion("1.0")]
[Tags("Applications")]
public sealed class DeployApplicationController : ControllerBase
{
    /// <summary>
    /// Deploys an application bundle to a specified site and application path.
    /// </summary>
    /// <param name="siteId">The ID of the site containing the application.</param>
    /// <param name="appId">The ID of the application to deploy.</param>
    /// <param name="forceDelete">Deletes all the contents of the target application.</param>
    /// <param name="bundle">The application bundle to deploy.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="Results{TSuccess, TFailure}"/> object representing the result of the deployment.
    /// The success value is <see cref="Ok"/> if the deployment was successful.
    /// The failure value is <see cref="NotFound{ProblemDetails}"/> if the site or application was not found.
    /// </returns>
    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Deploy(
        [FromRoute] long siteId,
        [FromRoute] string appId,
        [FromQuery] bool forceDelete,
        [FromForm] IFormFile bundle,
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

        await using var stream = new MemoryStream();
        await bundle.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        var sitePath = app.VirtualDirectories["/"].PhysicalPath;
        if (forceDelete && Directory.Exists(sitePath))
            Directory.Delete(sitePath, recursive: true);

        ZipFile.ExtractToDirectory(
            source: stream,
            destinationDirectoryName: sitePath,
            entryNameEncoding: null,
            overwriteFiles: true
        );

        return Ok();
    }
}