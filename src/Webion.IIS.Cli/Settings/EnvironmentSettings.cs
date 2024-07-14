using System.Collections.Frozen;
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
    public string? EnvFile { get; init; }


    public string AppId => Base64Id.Serialize(AppPath);

    private FrozenDictionary<string, string?>? _envVars;

    public async Task<FrozenDictionary<string, string?>> LoadEnvVarsAsync(CancellationToken cancellationToken)
    {
        if (_envVars is not null)
            return _envVars;
        
        var envFileLines = EnvFile is not null
            ? await File.ReadAllLinesAsync(EnvFile, cancellationToken)
            : [];

        var envVars = envFileLines
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Split('=', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Where(x => x.Length == 2)
            .ToFrozenDictionary<string[], string, string?>(
                x => x[0],
                x => x[1]
            );

        _envVars = envVars;
        return _envVars;
    }
}