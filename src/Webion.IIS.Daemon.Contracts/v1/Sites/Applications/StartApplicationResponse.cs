using Microsoft.Web.Administration;

namespace Webion.IIS.Daemon.Contracts.v1.Sites.Applications;

public sealed class StartApplicationResponse
{
    public required ObjectState State { get; init; }
}