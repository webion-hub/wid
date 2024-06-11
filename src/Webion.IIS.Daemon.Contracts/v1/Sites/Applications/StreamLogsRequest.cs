using System.ComponentModel.DataAnnotations;

namespace Webion.IIS.Daemon.Contracts.v1.Sites.Applications;

public sealed class StreamLogsRequest
{
    [Required]
    public long SiteId { get; init; }
    
    [Required]
    [Base64String]
    public string AppId { get; init; } = null!;
    
    [Required]
    public string LogDirectory { get; init; } = null!;
}