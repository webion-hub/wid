using Refit;
using Webion.IIS.Daemon.Contracts.v1.Sites;

namespace Webion.IIS.Client.Apis;

public interface ISitesApi
{
    [Get("/v{version:apiVersion}/sites")]
    Task<GetAllSitesResponse> GetAllAsync(CancellationToken cancellationToken);

    [Put("/v{version:apiVersion}/sites/{siteId}/start")]
    Task<ApiResponse<StartSiteResponse>> StartAsync(long siteId, CancellationToken cancellationToken);
    
    [Put("/v{version:apiVersion}/sites/{siteId}/stop")]
    Task<ApiResponse<StopSiteResponse>> StopAsync(long siteId, CancellationToken cancellationToken);
}