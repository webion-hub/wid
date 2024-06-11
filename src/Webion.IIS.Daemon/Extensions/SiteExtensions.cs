using Microsoft.Web.Administration;

namespace Webion.IIS.Daemon.Extensions;

public static class SiteExtensions
{
    public static async Task StartAsync(this Site site, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        do
        {
            if (site.State is ObjectState.Started)
                return;

            if (site.State is ObjectState.Starting)
                continue;

            site.Start();
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }
    
    public static async Task StopAsync(this Site site, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        do
        {
            if (site.State is ObjectState.Stopped)
                return;

            if (site.State is ObjectState.Stopping)
                continue;

            site.Start();
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }
}