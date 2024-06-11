using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Contracts.v1.AppPools;
using Webion.IIS.Daemon.Extensions;

namespace Webion.IIS.Daemon.Controllers.v1.AppPools;

[ApiController]
[Route("v1/app-pools/{appPoolId}")]
public sealed class StartAppPoolController : ControllerBase
{
    [HttpPut]
    public async Task<Results<Ok<StartAppPoolResponse>, NotFound>> Start(
        [FromRoute] string appPoolId,
        CancellationToken cancellationToken
    )
    {
        using var iis = new ServerManager();
        var name = Base64Id.Deserialize(appPoolId);

        var appPool = iis.ApplicationPools.FirstOrDefault(x => x.Name == name);
        if (appPool is null)
            return TypedResults.NotFound();

        await appPool.StartAsync(cancellationToken);
        return TypedResults.Ok(new StartAppPoolResponse
        {
            State = appPool.State,
        });
    }
}