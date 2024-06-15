namespace Webion.IIS.Daemon.Config.OpenApi.Tags;

public sealed class SitesTagGroup : ITagGroup
{
    public string Name => "Sites API";

    public IEnumerable<string> Tags =>
    [
        "Sites",
        "Applications",
    ];

    public IEnumerable<TagDefinition> TagDefinitions =>
    [
        new TagDefinition("Sites", "Sites"),
        new TagDefinition("Applications", "Applications")
    ];
}