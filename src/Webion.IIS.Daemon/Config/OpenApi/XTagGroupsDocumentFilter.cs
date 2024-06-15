using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Webion.IIS.Daemon.Config.OpenApi.Tags;

namespace Webion.IIS.Daemon.Config.OpenApi;

/// <summary>
/// ReDoc extension for grouping tags.
/// </summary>
public class XTagGroupsDocumentFilter<T> : IDocumentFilter
    where T: ITagGroup, new()
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var group = new T();

        if (!swaggerDoc.Extensions.ContainsKey("tags"))
            swaggerDoc.Extensions["tags"] = new OpenApiArray();
        
        if (!swaggerDoc.Extensions.ContainsKey("x-tagGroups"))
            swaggerDoc.Extensions["x-tagGroups"] = new OpenApiArray();

        if (swaggerDoc.Extensions["tags"] is OpenApiArray tags)
        {
            tags.AddRange(group.TagDefinitions.Select(x => new OpenApiObject
            {
                ["name"] = new OpenApiString(x.Name),
                ["x-displayName"] = new OpenApiString(x.DisplayName),
            }));
        }

        if (swaggerDoc.Extensions["x-tagGroups"] is OpenApiArray tagGroups)
        {
            var groupTags = new OpenApiArray();
            groupTags.AddRange(group.Tags.Select(x => new OpenApiString(x)));
            
            tagGroups.Add(new OpenApiObject
            {
                ["name"] = new OpenApiString(group.Name),
                ["tags"] = groupTags,
            });
        }
    }
}