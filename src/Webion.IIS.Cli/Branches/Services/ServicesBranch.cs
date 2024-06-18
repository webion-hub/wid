using Spectre.Console.Cli;
using Webion.IIS.Cli.Branches.Services.Build;
using Webion.IIS.Cli.Branches.Services.Deploy;
using Webion.IIS.Cli.Branches.Services.Logging.Read;
using Webion.IIS.Cli.Branches.Services.Logging.Stream;
using Webion.IIS.Cli.Branches.Services.Recycle;
using Webion.IIS.Cli.Branches.Services.Start;
using Webion.IIS.Cli.Branches.Services.Stop;

namespace Webion.IIS.Cli.Branches.Services;

public sealed class ServicesBranch : IBranchConfig
{
    public void Configure(IConfigurator config)
    {
        config.AddBranch("services", services =>
        {
            services.AddCommand<StartServiceCommand>("start");
            services.AddCommand<StopServiceCommand>("stop");
            services.AddCommand<RecycleServiceCommand>("recycle");
            services.AddCommand<BuildCommand>("build");
            services.AddCommand<DeployCommand>("deploy");
            
            services.AddBranch("logs", logs =>
            {
                logs.AddCommand<ReadLogsCommand>("read");
                logs.AddCommand<StreamLogsCommand>("stream");
            });
        }).WithAlias("service");
    }
}