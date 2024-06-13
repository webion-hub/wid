namespace Webion.IIS.Daemon.Contracts.v1.Sites.Applications;

public sealed class BuildDto
{
    public required string Command { get; init; }
    public required IEnumerable<string> Args { get; init; }
    public required string WorkDir { get; init; }
}