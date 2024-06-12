using Spectre.Console.Cli;

namespace Webion.IIS.Cli.Branches.Services.Stop;

public sealed class StopServiceBranch : IBranchConfig
{
    public void Configure(IConfigurator config)
    {
        config.AddCommand<StopServiceCommand>("stop");
    }
}