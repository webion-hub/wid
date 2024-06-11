using Refit;
using Webion.IIS.Client.Apis;

namespace Webion.IIS.Client;

internal sealed class IISDaemonClient : IIISDaemonClient
{
    private readonly HttpClient _client;

    public IISDaemonClient(HttpClient client)
    {
        _client = client;
    }

    public IApplicationsApi Applications => RestService.For<IApplicationsApi>(_client);
    public ISitesApi Sites => RestService.For<ISitesApi>(_client);
    public IAppPoolsApi AppPools => RestService.For<IAppPoolsApi>(_client);

    public Uri? BaseAddress
    {
        get => _client.BaseAddress;
        set => _client.BaseAddress = value;
    }
}