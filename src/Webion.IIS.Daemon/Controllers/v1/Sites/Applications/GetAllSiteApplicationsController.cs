using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Core.ValueObjects;
using Webion.IIS.Daemon.Controllers.v1.Sites.Applications.GetAll;

namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications;

[ApiController]
[Route("v1/sites/{siteId:long}/applications")]
public sealed class GetAllSiteApplicationsController : ControllerBase
{
    [HttpGet]
    public Results<Ok<GetAllSiteApplicationsResponse>, NotFound> GetAll([FromRoute] long siteId)
    {
        using var iis = new ServerManager();

        var site = iis.Sites.FirstOrDefault(x => x.Id == siteId);
        if (site is null)
            return TypedResults.NotFound();

        var apps = site.Applications
            .Select(x => new ApplicationDto
            {
                Id = Base64Id.Serialize(x.Path),
                Path = x.Path,
                AppPoolName = x.ApplicationPoolName,
                EnabledProtocols = x.EnabledProtocols,
            })
            .ToList();

        return TypedResults.Ok(new GetAllSiteApplicationsResponse
        {
            Applications = apps,
        });
    }
}