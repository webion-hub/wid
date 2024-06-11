using Microsoft.Web.Administration;

namespace Webion.IIS.Daemon.Contracts.v1.AppPools;

public sealed class GetAllAppPoolsResponse
{
    public required IEnumerable<ApplicationPoolDto> AppPools { get; init; }
}

public sealed class ApplicationPoolDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required ObjectState State { get; init; }
    public required bool AutoStart { get; init; }
    public required StartMode StartMode { get; init; }
    public required long QueueLength { get; init; }
    public required string ManagedRuntimeVersion { get; init; }
    public required bool Enable32BitAppOnWin64 { get; init; }
}