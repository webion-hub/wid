namespace Webion.IIS.Daemon.Config.OpenApi.Tags;

public sealed class AppPoolsTagGroup : ITagGroup
{
    public string Name => "App Pools API";

    public IEnumerable<string> Tags =>
    [
        "App Pools",
    ];

    public IEnumerable<TagDefinition> TagDefinitions =>
    [
        new TagDefinition("App Pools", "App Pools")
    ];
}