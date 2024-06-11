using Spectre.Console.Cli;

namespace Webion.IIS.Cli.Branches;

public interface IBranchConfig
{
    void Configure(IConfigurator config);
}