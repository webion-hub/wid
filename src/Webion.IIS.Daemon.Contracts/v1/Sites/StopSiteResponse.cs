using Microsoft.Web.Administration;

namespace Webion.IIS.Daemon.Contracts.v1.Sites;

public sealed class StopSiteResponse
{
    public required ObjectState State { get; init; }
}