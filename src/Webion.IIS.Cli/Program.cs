using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using Spectre.Console.Cli;
using SpectreConsoleLogger;
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

services.AddLogging(options =>
{
    options.ClearProviders();
    options.AddProvider(new SpectreConsoleLoggerProvider());
    
    var logLevel = config.GetValue("log-level", LogLevel.Error);
    options.SetMinimumLevel(logLevel);
});

services.AddIISDaemonClient();
services.AddSingleton<ICliApplicationLifetime, CliApplicationLifetime>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(o =>
{
    var fullErrors = config.GetValue("full-errors", false);
    if (fullErrors)
        o.PropagateExceptions();
    
    o.SetApplicationVersion("0.2-alpha");
    o.AddBranch<ServicesBranch>();
});

return await app.RunAsync(args);