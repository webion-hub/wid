using Spectre.Console.Cli;

namespace Webion.IIS.Cli.Branches.Build;

public sealed class BuildBranch : IBranchConfig
{
    public void Configure(IConfigurator config)
    {
        config.AddCommand<BuildCommand>("build");
    }
}