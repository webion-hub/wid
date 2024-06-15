namespace Webion.IIS.Cli.Settings;

public sealed class BuildSettings
{
    public required string Command { get; init; }
    public required string[] Args { get; init; }
    public required string WorkDir { get; init; }
}