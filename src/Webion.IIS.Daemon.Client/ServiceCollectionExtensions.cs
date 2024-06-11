using Microsoft.Extensions.DependencyInjection;

namespace Webion.IIS.Client;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddIISDaemonClient(this IServiceCollection services)
    {
        return services.AddHttpClient<IIISDaemonClient, IISDaemonClient>();
    }
}