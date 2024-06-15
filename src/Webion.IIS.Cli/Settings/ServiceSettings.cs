using Webion.IIS.Daemon.Contracts.v1.Sites.Applications;

namespace Webion.IIS.Cli.Settings;

public sealed class ServiceSettings
{
    public required bool IsProduction { get; init; }
    public required Uri DaemonAddress { get; init; }
    public required long SiteId { get; init; }
    public required string AppPath { get; init; }

    public required bool ForceDelete { get; init; }
    public required string BundleDir { get; init; }
    public required string LogDir { get; init; }

    public required BuildSettings Build { get; init; }
}