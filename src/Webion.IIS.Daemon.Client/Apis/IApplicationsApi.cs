using Refit;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications;
using Webion.IIS.Daemon.Controllers.v1.Sites.Applications.GetAll;

namespace Webion.IIS.Client.Apis;

public interface IApplicationsApi
{
    [Get("/v1/sites/{siteId}/applications")]
    Task<ApiResponse<GetAllSiteApplicationsResponse>> GetAllAsync(long siteId);

    [Multipart]
    [Post("/v1/sites/{siteId}/applications/{appId}/deploy")]
    Task<IApiResponse> DeployAsync(
        long siteId,
        string appId,
        [Query] bool deleteDirectory,
        [AliasAs("bundle")] StreamPart bundle,
        CancellationToken cancellationToken
    );

    [Put("/v1/sites/{siteId}/applications/{appId}/start")]
    Task<ApiResponse<StartApplicationResponse>> StartAsync(long siteId, string appId);

    [Put("/v1/sites/{siteId}/applications/{appId}/stop")]
    Task<ApiResponse<StopApplicationResponse>> StopAsync(long siteId, string appId);
}