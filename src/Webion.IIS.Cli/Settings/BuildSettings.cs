namespace Webion.IIS.Cli.Settings;

public sealed class BuildSettings
{
    public string Name { get; init; } = null!;
    public string Run { get; init; } = null!;
    public string WorkDir { get; init; } = ".";
    public bool EnsureNonZero { get; set; } = false;
    public string[] OnEnvs { get; set; } = [];
}