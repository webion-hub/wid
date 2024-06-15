namespace Webion.IIS.Daemon.Config.OpenApi.Tags;

public interface ITagGroup
{
    string Name { get; }
    IEnumerable<string> Tags { get; }
    IEnumerable<TagDefinition> TagDefinitions { get; }
}

public sealed record TagDefinition(string Name, string DisplayName);