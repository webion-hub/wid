using System.Text.Json;
using Refit;
using Spectre.Console;

namespace Webion.IIS.Cli.Ui.Errors;

public static class ApiErrorTable
{
    public static Table From(IApiResponse response)
    {
        var table = new Table();
        table.AddColumns("", "");
        table.AddRow("Status", response.StatusCode.ToString());

        if (response.Error is null)
            return table;

        try
        {
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(response.Error.Content ?? "{}");
            table.AddRow("Title", problemDetails?.Title ?? "");
            table.AddRow("Detail", problemDetails?.Detail ?? "");
        }
        catch
        {
            table.AddRow("Error text", response.Error.Content ?? "");
        }

        return table;
    }
}