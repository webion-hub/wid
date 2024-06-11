using Refit;
using Webion.IIS.Daemon.Contracts.v1.AppPools;

namespace Webion.IIS.Client.Apis;

public interface IAppPoolsApi
{
    [Get("/v1/app-pools")]
    Task<GetAllAppPoolsResponse> GetAllAsync();

    [Put("/v1/app-pools/{appPoolId}/start")]
    Task<ApiResponse<StartAppPoolResponse>> StartAsync(string appPoolId);
    
    [Put("/v1/app-pools/{appPoolId}/stop")]
    Task<ApiResponse<StartAppPoolResponse>> StopAsync(string appPoolId);
}