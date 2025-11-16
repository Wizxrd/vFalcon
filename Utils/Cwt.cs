using Newtonsoft.Json.Linq;
using System.IO;
namespace vFalcon.Utils;

public class Cwt
{
    private static string path = Path.Combine(PathFinder.GetAppDirectory(), "AircraftCwt.json");
    private static string json = File.ReadAllText(path);
    private static JArray jArray = JArray.Parse(json);

    public static string GetCodeFromType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return string.Empty;

        var match = jArray
            .FirstOrDefault(o => string.Equals(
                (string?)o["typeCode"],
                type,
                StringComparison.OrdinalIgnoreCase));

        return match?["cwtCode"]?.Value<string>() ?? string.Empty;
    }
}
