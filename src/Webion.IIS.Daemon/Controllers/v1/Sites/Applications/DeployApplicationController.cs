using System.IO.Compression;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Core.ValueObjects;

namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications;

[ApiController]
[Route("v1/sites/{siteId:long}/applications/{appId}/deploy")]
public sealed class DeployApplicationController : ControllerBase
{
    /// <summary>
    /// Deploys an application bundle to a specified site and application path.
    /// </summary>
    /// <param name="siteId">The ID of the site to deploy the application to.</param>
    /// <param name="appId">The ID of the application to deploy.</param>
    /// <param name="deleteDirectory">Delete the application directory to deploy.</param>
    /// <param name="bundle">The application bundle to deploy.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="Results{TSuccess, TFailure}"/> object representing the result of the deployment.
    /// The success value is <see cref="Ok"/> if the deployment was successful.
    /// The failure value is <see cref="NotFound{ProblemDetails}"/> if the site or application was not found.
    /// </returns>
    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<Results<Ok, NotFound<ProblemDetails>>> Deploy(
        [FromRoute] long siteId,
        [FromRoute] string appId,
        [FromQuery] bool deleteDirectory,
        [FromForm] IFormFile bundle,
        CancellationToken cancellationToken
    )
    {
        using var iis = new ServerManager();

        var site = iis.Sites.FirstOrDefault(x => x.Id == siteId);
        if (site is null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Detail = "Could not find the given site",
                Status = StatusCodes.Status404NotFound,
            });
        }

        var path = Base64Id.Deserialize(appId);
        var app = site.Applications.FirstOrDefault(x => x.Path == path);
        if (app is null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Detail = "Could not find the application for the given site",
                Status = StatusCodes.Status404NotFound,
            });
        }

        await using var stream = new MemoryStream();
        await bundle.CopyToAsync(stream, cancellationToken);
        stream.Position = 0;

        var sitePath = Path.Combine(app.VirtualDirectories["/"].PhysicalPath);

        if (deleteDirectory && Directory.Exists(sitePath))
            Directory.Delete(sitePath, true);

        ZipFile.ExtractToDirectory(
            source: stream,
            destinationDirectoryName: sitePath,
            entryNameEncoding: null,
            overwriteFiles: true
        );

        return TypedResults.Ok();
    }
}