using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Contracts.v1.AppPools;

namespace Webion.IIS.Daemon.Controllers.v1.AppPools;

[ApiController]
[Route("v{version:apiVersion}/app-pools")]
[ApiVersion("1.0")]
[Tags("App Pools")]
public sealed class GetAllAppPoolsController : ControllerBase
{
    [HttpGet]
    public Ok<GetAllAppPoolsResponse> GetAll()
    {
        using var iis = new ServerManager();
        var appPools = iis.ApplicationPools
            .Select(x => new ApplicationPoolDto
            {
                Id = Base64Id.Serialize(x.Name),
                Name = x.Name,
                State = x.State,
                AutoStart = x.AutoStart,
                StartMode = x.StartMode,
                QueueLength = x.QueueLength,
                ManagedRuntimeVersion = x.ManagedRuntimeVersion,
                Enable32BitAppOnWin64 = x.Enable32BitAppOnWin64,
            })
            .ToList();

        return TypedResults.Ok(new GetAllAppPoolsResponse
        {
            AppPools = appPools,
        });
    }
}