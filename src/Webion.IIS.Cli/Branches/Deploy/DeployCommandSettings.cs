using System.ComponentModel;
using Spectre.Console.Cli;

namespace Webion.IIS.Cli.Branches.Deploy;

public sealed class DeployCommandSettings : CommandSettings
{
    [Description("Name of the service to deploy")]
    [CommandArgument(0, "<ServiceName>")]
    public string ServiceName { get; init; } = null!;
    
    [Description("Path to the settings file")]
    [CommandOption("--settings-file")]
    public string? SettingsFile { get; init; }
}