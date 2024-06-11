namespace Webion.IIS.Cli.Core;

public interface ICliApplicationLifetime
{
    CancellationToken CancellationToken { get; }
}