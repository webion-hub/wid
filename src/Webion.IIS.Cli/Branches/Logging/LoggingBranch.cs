using Spectre.Console.Cli;
using Webion.IIS.Cli.Branches.Logging.Stream;

namespace Webion.IIS.Cli.Branches.Logging;

public sealed class LoggingBranch : IBranchConfig
{
    public void Configure(IConfigurator config)
    {
        config.AddBranch("logs", logs =>
        {
            logs.AddCommand<StreamLogsCommand>("stream");
        });
    }
}