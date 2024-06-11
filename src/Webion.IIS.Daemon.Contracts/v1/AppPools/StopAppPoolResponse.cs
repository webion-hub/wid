using Microsoft.Web.Administration;

namespace Webion.IIS.Daemon.Contracts.v1.AppPools;

public sealed class StopAppPoolResponse
{
    public required ObjectState State { get; init; }
}