using Microsoft.Extensions.DependencyInjection;
using Webion.Extensions.DependencyInjection;

namespace Webion.IIS.Cli.Branches.Services.Build;

public sealed class BuildModule : IModule
{
    public void Configure(IServiceCollection services)
    {
        services.AddTransient<ServiceBuilder>();
    }
}