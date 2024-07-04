namespace Webion.IIS.Daemon.Controllers.v1.Sites.Applications.Archive;

public sealed class GetAllBackupsResponse
{
    public required IEnumerable<BackupFolderDto> Backups { get; init; }
}

public sealed class BackupFolderDto
{
    public required string Name { get; init; }
    public required long BytesSize { get; init; }
    public required DateTimeOffset CreatedAtUtc { get; init; }
}