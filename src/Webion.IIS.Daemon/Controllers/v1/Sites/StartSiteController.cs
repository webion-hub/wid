using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Daemon.Contracts.v1.Sites;
using Webion.IIS.Daemon.Extensions;

namespace Webion.IIS.Daemon.Controllers.v1.Sites;

[ApiController]
[Route("v{version:apiVersion}/sites/{siteId:long}/start")]
[ApiVersion("1.0")]
[Tags("Sites")]
public sealed class StartSiteController : ControllerBase
{
    [HttpPut]
    public async Task<Results<Ok<StartSiteResponse>, NotFound>> Start(
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
        
        await site.StartAsync(cancellationToken);
        return TypedResults.Ok(new StartSiteResponse
        {
            State = site.State,
        });
    }
}