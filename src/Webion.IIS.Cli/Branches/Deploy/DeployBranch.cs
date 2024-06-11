using Spectre.Console.Cli;

namespace Webion.IIS.Cli.Branches.Deploy;

public sealed class DeployBranch : IBranchConfig
{
    public void Configure(IConfigurator config)
    {
        config.AddCommand<DeployCommand>("deploy");
    }
}