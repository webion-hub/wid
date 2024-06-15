using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Spectre.Console.Cli;
using Webion.IIS.Cli.Branches;
using Webion.IIS.Cli.Branches.Services;
using Webion.IIS.Cli.Branches.Services.Deploy;
using Webion.IIS.Cli.Branches.Services.Logging;
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
    o.AddBranch<ServicesBranch>();
});

return await app.RunAsync(args);