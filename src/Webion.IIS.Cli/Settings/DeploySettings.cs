using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Webion.IIS.Cli.Settings;

public sealed class DeploySettings
{
    public required string Version { get; init; }
    public required Dictionary<string, ServiceSettings> Services { get; init; }


    public static async Task<ServiceSettings?> GetServiceFromFileAsync(
        string path,
        string serviceName,
        CancellationToken cancellationToken
    )
    {
        var settings = await TryReadFromFileAsync(path, cancellationToken);
        return settings.Services.GetValueOrDefault(serviceName);
    }

    private static async Task<DeploySettings> TryReadFromFileAsync(string path, CancellationToken cancellationToken)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var settingsFile = await File.ReadAllTextAsync(path, cancellationToken);
        return deserializer.Deserialize<DeploySettings>(settingsFile);
    }
}