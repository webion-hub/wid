using System.ComponentModel;
using Spectre.Console.Cli;

namespace Webion.IIS.Cli.Branches.Services.Build;

public sealed class BuildCommandSettings : CommandSettings
{
    [Description("Name of the service to deploy")]
    [CommandArgument(0, "<ServiceName>")]
    public string ServiceName { get; init; } = null!;
    
    [Description("Path to the settings file")]
    [CommandOption("--settings-file")]
    public string? SettingsFile { get; init; }
}