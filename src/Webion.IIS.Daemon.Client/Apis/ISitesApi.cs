using Refit;
using Webion.IIS.Daemon.Contracts.v1.Sites;

namespace Webion.IIS.Client.Apis;

public interface ISitesApi
{
    [Get("/v1/sites")]
    Task<GetAllSitesResponse> GetAllAsync(CancellationToken cancellationToken);

    [Put("/v1/sites/{siteId}/start")]
    Task<ApiResponse<StartSiteResponse>> StartAsync(long siteId, CancellationToken cancellationToken);
    
    [Put("/v1/sites/{siteId}/stop")]
    Task<ApiResponse<StopSiteResponse>> StopAsync(long siteId, CancellationToken cancellationToken);
}