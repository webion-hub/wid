using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Branches;
using Webion.IIS.Cli.Branches.Deploy;
using Webion.IIS.Cli.DI;
using Webion.IIS.Client;

var config = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

var services = new ServiceCollection();

services.AddIISDaemonClient();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(o =>
{
    o.AddBranch<DeployBranch>();
});

return await app.RunAsync(args);