using Microsoft.Web.Administration;

namespace Webion.IIS.Daemon.Contracts.v1.Sites;

public sealed class GetAllSitesResponse
{
    public required IEnumerable<SiteDto> Sites { get; init; }
}

public sealed class SiteDto
{
    public required long Id { get; init; }
    public required string Name { get; init; }
    public required ObjectState State { get; init; }
}