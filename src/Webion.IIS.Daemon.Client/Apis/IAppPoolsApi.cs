using Refit;
using Webion.IIS.Daemon.Contracts.v1.AppPools;

namespace Webion.IIS.Client.Apis;

public interface IAppPoolsApi
{
    [Get("/v{version:apiVersion}/app-pools")]
    Task<GetAllAppPoolsResponse> GetAllAsync();

    [Put("/v{version:apiVersion}/app-pools/{appPoolId}/start")]
    Task<ApiResponse<StartAppPoolResponse>> StartAsync(string appPoolId);
    
    [Put("/v{version:apiVersion}/app-pools/{appPoolId}/stop")]
    Task<ApiResponse<StartAppPoolResponse>> StopAsync(string appPoolId);
}