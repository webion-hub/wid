namespace Webion.IIS.Daemon.Contracts.v1.Sites.Applications.Logging;

public sealed class GetAllLogsResponse
{
    public required IEnumerable<LogFileDto> LogFiles { get; init; }
}

public sealed class LogFileDto
{
    public required string Name { get; init; }
    public required DateTime LastWriteTimeUtc { get; init; }
    public required long BytesSize { get; init; }
}