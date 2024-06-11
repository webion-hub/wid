using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications;
using Webion.IIS.Daemon.Extensions;

namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications;

[ApiController]
[Route("v1/sites/{siteId:long}/applications/{appId}/start")]
public sealed class StartApplicationController : ControllerBase
{
    [HttpPut]
    public async Task<Results<Ok<StartApplicationResponse>, NotFound<ProblemDetails>>> Start(
        [FromRoute] long siteId,
        [FromRoute] string appId,
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

        var appPool = iis.ApplicationPools
            .Where(x => x.Name == app.ApplicationPoolName)
            .FirstOrDefault();

        if (appPool is null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Detail = "Could not find the app pool for the given application",
                Status = StatusCodes.Status404NotFound,
            });
        }

        await appPool.StartAsync(cancellationToken);
            
        return TypedResults.Ok(new StartApplicationResponse
        {
            State = appPool.State,
        });
    }
}