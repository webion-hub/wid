using Microsoft.Web.Administration;
using Webion.IIS.Core.ValueObjects;

namespace Webion.IIS.Daemon.Managers;

public sealed class IISManager : IDisposable
{
    private readonly ServerManager _iis = new();
    
    public FindAppResult FindApp(long siteId, string appId)
    {
        var site = _iis.Sites.FirstOrDefault(x => x.Id == siteId);
        if (site is null)
            return new FindAppResult.SiteNotFound();

        var path = Base64Id.Deserialize(appId);
        var app = site.Applications.FirstOrDefault(x => x.Path == path);
        if (app is null)
            return new FindAppResult.AppNotFound();

        return new FindAppResult.Found(site, app);
    }

    public void Dispose()
    {
        _iis.Dispose();
    }
}