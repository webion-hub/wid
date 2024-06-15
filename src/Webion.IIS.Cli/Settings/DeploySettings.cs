using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Webion.IIS.Cli.Settings;

public sealed class DeploySettings
{
    public required string Version { get; init; }
    public required Dictionary<string, ServiceSettings> Services { get; init; }

    public static async Task<DeploySettings> TryReadFromFileAsync(string path)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var settingsFile = await File.ReadAllTextAsync(path);
        return deserializer.Deserialize<DeploySettings>(settingsFile);
    }
}