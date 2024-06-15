using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Daemon.Contracts.v1.Sites;
using Webion.IIS.Daemon.Extensions;

namespace Webion.IIS.Daemon.Controllers.v1.Sites;

[ApiController]
[Route("v{version:apiVersion}/sites/{siteId:long}/stop")]
[ApiVersion("1.0")]
[Tags("Sites")]
public sealed class StopSiteController : ControllerBase
{
    [HttpPut]
    public async Task<Results<Ok<StopSiteResponse>, NotFound>> Stop(
        [FromRoute] long siteId,
        CancellationToken cancellationToken
    )
    {
        using var iis = new ServerManager();
        var site = iis.Sites
            .Where(x => x.Id == siteId)
            .FirstOrDefault();

        if (site is null)
            return TypedResults.NotFound();
        
        await site.StopAsync(cancellationToken);
        return TypedResults.Ok(new StopSiteResponse
        {
            State = site.State,
        });
    }
}