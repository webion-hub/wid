using Refit;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications;
using Webion.IIS.Daemon.Contracts.v1.Sites.Applications.Logging;

namespace Webion.IIS.Client.Apis;

public interface IApplicationsApi
{
    [Get("/v1/applications")]
    Task<ApiResponse<GetAllSiteApplicationsResponse>> GetAllAsync(CancellationToken cancellationToken);

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
    Task<ApiResponse<StartApplicationResponse>> StartAsync(
        long siteId,
        string appId,
        CancellationToken cancellationToken
    );

    [Put("/v1/sites/{siteId}/applications/{appId}/stop")]
    Task<ApiResponse<StopApplicationResponse>> StopAsync(
        long siteId,
        string appId,
        CancellationToken cancellationToken
    );

    [Get("/v1/sites/{siteId}/applications/{appId}/logs")]
    Task<ApiResponse<GetAllLogsResponse>> EnumerateLogsAsync(
        long siteId,
        string appId,
        [Query] string logsPath,
        CancellationToken cancellationToken
    );
    
    [Get("/v1/sites/{siteId}/applications/{appId}/logs/{base64FileName}")]
    Task<ApiResponse<Stream>> ReadLogAsync(
        long siteId,
        string appId,
        string base64FileName,
        CancellationToken cancellationToken
    );
}