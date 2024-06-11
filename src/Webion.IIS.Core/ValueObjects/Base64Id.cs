using System.Text;

namespace Webion.IIS.Core.ValueObjects;

public static class Base64Id
{
    public static string Serialize(string v)
    {
        var bytes = Encoding.UTF8.GetBytes(v);
        var base64 = Convert.ToBase64String(bytes);

        return base64;
    }

    public static string Deserialize(string base64String)
    {
        var bytes = Convert.FromBase64String(base64String);
        var result = Encoding.UTF8.GetString(bytes);

        return result;
    }
}