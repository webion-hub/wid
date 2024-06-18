using System.Text.Json;
using Refit;
using Spectre.Console;
using Spectre.Console.Json;
using Spectre.Console.Rendering;

namespace Webion.IIS.Cli.Ui.Errors;

public static class ApiErrorControl
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };
    
    public static Renderable From(IApiResponse response)
    {
        if (string.IsNullOrWhiteSpace(response.Error?.Content))
        {
            var json = new { response.StatusCode };
            return new JsonText(JsonSerializer.Serialize(json, JsonSerializerOptions));
        }

        try
        {
            var json = response.Error.Content;
            var error = JsonSerializer.Deserialize<ProblemDetails>(json);
            var result = JsonSerializer.Serialize(error, JsonSerializerOptions);

            return new JsonText(result);
        }
        catch
        {
            return new Text(response.Error.Content);
        }
    }
}