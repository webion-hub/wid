using Microsoft.Extensions.DependencyInjection;

namespace Webion.IIS.Client;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddIisDaemonClient(this IServiceCollection services)
    {
        return services.AddHttpClient<IIisDaemonClient, IisDaemonClient>();
    }
}