using Refit;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications;

namespace Webion.IIS.Client.Apis;

public interface IApplicationsApi
{
    [Get("/v1/applications")]
    Task<ApiResponse<GetAllSiteApplicationsResponse>> GetAllAsync();

    [Multipart]
    [Post("/v1/sites/{siteId}/applications/{appId}/deploy")]
    Task<IApiResponse> DeployAsync(
        long siteId,
        string appId,
        [Query] bool forceDelete,
        [AliasAs("bundle")] StreamPart bundle,
        CancellationToken cancellationToken
    );

    [Put("/v1/sites/{siteId}/applications/{appId}/start")]
    Task<ApiResponse<StartApplicationResponse>> StartAsync(long siteId, string appId);

    [Put("/v1/sites/{siteId}/applications/{appId}/stop")]
    Task<ApiResponse<StopApplicationResponse>> StopAsync(long siteId, string appId);
}