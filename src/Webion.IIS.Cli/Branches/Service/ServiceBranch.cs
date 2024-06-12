using Spectre.Console.Cli;
using Webion.IIS.Cli.Branches.Service.Start;
using Webion.IIS.Cli.Branches.Service.Stop;

namespace Webion.IIS.Cli.Branches.Service;

public sealed class ServiceBranch : IBranchConfig
{
    public void Configure(IConfigurator config)
    {
        config.AddCommand<StartServiceCommand>("start");
        config.AddCommand<StopServiceCommand>("stop");
    }
}