namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications.GetAll;

public sealed class GetAllSiteApplicationsResponse
{
    public required IEnumerable<ApplicationDto> Applications { get; init; }
}

public sealed class ApplicationDto
{
    public required string Id { get; init; }
    public required string Path { get; set; }
    public required string AppPoolName { get; init; }
    public required string EnabledProtocols { get; set; }
}