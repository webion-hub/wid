using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Webion.AspNetCore;
using Webion.IIS.Daemon.Config.OpenApi.Tags;

namespace Webion.IIS.Daemon.Config.OpenApi;

public sealed class OpenApiConfig : IWebApplicationConfiguration
{
    public void Add(WebApplicationBuilder builder)
    {
        builder.Services.ConfigureOptions<ApiVersioningOptions>();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.CustomSchemaIds(x => x.FullName);

            options.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "duration", Example = new OpenApiString("00:45:00"), });
            
            options.DocumentFilter<XTagGroupsDocumentFilter<SitesTagGroup>>();
            options.DocumentFilter<XTagGroupsDocumentFilter<AppPoolsTagGroup>>();
        });
    }

    public void Use(WebApplication app)
    {
        app.UseSwagger();
        app.MapGet("/v{version:apiVersion}/docs", (string version) =>
        {
            return Results.Content(
                content:
                $$"""
                <!doctype html>
                <html>
                <head>
                    <title>Qubi Api Reference -- {{version}}</title>
                    <meta charset="utf-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                </head>
                <body>
                    <script id="api-reference" data-url="/swagger/{{version}}/swagger.json"></script>
                    <script>
                        var configuration = {
                            theme: 'purple',
                            layout: 'modern',
                        }

                        document.getElementById('api-reference').dataset.configuration = JSON.stringify(configuration)
                    </script>
                    <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
                </body>
                </html>
                """,
                contentType: "text/html"
            );
        });
    }
}