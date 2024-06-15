using Webion.IIS.Core.ValueObjects;

namespace Webion.IIS.Cli.Settings;

public sealed class EnvironmentSettings
{
    public string Name { get; init; } = null!;
    public bool IsProduction { get; init; }
    public Uri DaemonAddress { get; init; } = null!;
    public long SiteId { get; init; }
    public string AppPath { get; init; } = null!;
    public string LogDir { get; init; } = null!;


    public string AppId => Base64Id.Serialize(AppPath);
}