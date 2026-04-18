using Webion.IIS.Client.Apis;

namespace Webion.IIS.Client;

public interface IIisDaemonClient
{
    public IApplicationsApi Applications { get; }
    public ISitesApi Sites { get; }
    public IAppPoolsApi AppPools { get; }
    
    public Uri? BaseAddress { get; set; }
}