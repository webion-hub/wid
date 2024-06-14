using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Contracts.v1.AppPools;
using Webion.IIS.Daemon.Contracts.v1.Sites;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications;

namespace Webion.IIS.Daemon.Controllers.v1.Applications;

[ApiController]
[Route("v1/applications")]
public sealed class GetAllSiteApplicationsController : ControllerBase
{
    [HttpGet]
    public Ok<GetAllSiteApplicationsResponse> GetAll()
    {
        using var iis = new ServerManager();

        var apps = iis.Sites
            .SelectMany(x => x.Applications, (Site, Application) => new
            {
                Site,
                Application
            })
            .Select(x => new ApplicationDto
            {
                Id = Base64Id.Serialize(x.Application.Path),
                Path = x.Application.Path,
                Site = new SiteDto
                {
                    Id = x.Site.Id,
                    Name = x.Site.Name,
                    State = x.Site.State
                },
                AppPool = iis.ApplicationPools
                    .Where(ap => x.Application.ApplicationPoolName == ap.Name)
                    .Select(ap => new ApplicationPoolDto
                    {
                        Id = Base64Id.Serialize(ap.Name),
                        Name = ap.Name,
                        State = ap.State,
                        AutoStart = ap.AutoStart,
                        StartMode = ap.StartMode,
                        QueueLength = ap.QueueLength,
                        ManagedRuntimeVersion = ap.ManagedRuntimeVersion,
                        Enable32BitAppOnWin64 = ap.Enable32BitAppOnWin64,
                    })
                    .First(),
                EnabledProtocols = x.Application.EnabledProtocols,
            })
            .ToList();

        return TypedResults.Ok(new GetAllSiteApplicationsResponse
        {
            Applications = apps,
        });
    }
}