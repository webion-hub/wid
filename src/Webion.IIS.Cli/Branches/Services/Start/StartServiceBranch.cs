using Spectre.Console.Cli;

namespace Webion.IIS.Cli.Branches.Services.Start;

public sealed class StartServiceBranch : IBranchConfig
{
    public void Configure(IConfigurator config)
    {
        config.AddCommand<StartServiceCommand>("start");
    }
}