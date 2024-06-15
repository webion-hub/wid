using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Web.Administration;
using Webion.IIS.Daemon.Contracts.v1.Sites;

namespace Webion.IIS.Daemon.Controllers.v1.Sites;

[ApiController]
[Route("v{version:apiVersion}/sites")]
[ApiVersion("1.0")]
[Tags("Sites")]
public sealed class GetAllSitesController : ControllerBase
{
    [HttpGet]
    public Ok<GetAllSitesResponse> GetAll()
    {
        using var iis = new ServerManager();
        var sites = iis.Sites
            .Select(x => new SiteDto
            {
                Id = x.Id,
                Name = x.Name,
                State = x.State,
            })
            .ToList();
        
        return TypedResults.Ok(new GetAllSitesResponse
        {
            Sites = sites,
        });
    }
}