using Microsoft.Web.Administration;

namespace Webion.IIS.Daemon.Extensions;

public static class ApplicationPoolExtensions
{
    public static async Task StopAsync(this ApplicationPool appPool, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        do
        {
            if (appPool.State is ObjectState.Stopped)
                return;

            if (appPool.State is ObjectState.Stopping)
                continue;

            appPool.Stop();
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }
    
    public static async Task StartAsync(this ApplicationPool appPool, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        do
        {
            if (appPool.State is ObjectState.Started)
                return;

            if (appPool.State is ObjectState.Starting)
                continue;

            appPool.Start();
        } while (await timer.WaitForNextTickAsync(cancellationToken));
    }
}