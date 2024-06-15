using Webion.IIS.Daemon.Contracts.v1.AppPools;

namespace Webion.IIS.Daemon.Contracts.v1.Sites.Applications;

public sealed class GetAllSiteApplicationsResponse
{
    public required IEnumerable<ApplicationDto> Applications { get; init; }
}

public sealed class ApplicationDto
{
    public required string Id { get; init; }
    public required string Path { get; set; }
    public required SiteDto Site { get; set; }
    public required ApplicationPoolDto AppPool { get; init; }
    public required string EnabledProtocols { get; set; }
}