using Microsoft.Web.Administration;

namespace Webion.IIS.Daemon.Managers;

public abstract record FindAppResult
{
    public sealed record SiteNotFound : FindAppResult;

    public sealed record AppNotFound : FindAppResult;

    public sealed record Found(Site Site, Application App) : FindAppResult;
}