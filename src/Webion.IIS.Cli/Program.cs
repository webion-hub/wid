using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Branches;
using Webion.IIS.Cli.Branches.Build;
using Webion.IIS.Cli.Branches.Deploy;
using Webion.IIS.Cli.Branches.Logging;
using Webion.IIS.Cli.Branches.Service;
using Webion.IIS.Cli.Core;
using Webion.IIS.Cli.DI;
using Webion.IIS.Client;

var config = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

var services = new ServiceCollection();

services.AddIISDaemonClient();
services.AddSingleton<ICliApplicationLifetime, CliApplicationLifetime>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(o =>
{
    o.AddBranch<BuildBranch>();
    o.AddBranch<DeployBranch>();
    o.AddBranch<LoggingBranch>();
    o.AddBranch<ServiceBranch>();
});

return await app.RunAsync(args);