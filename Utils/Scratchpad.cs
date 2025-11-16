using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
namespace vFalcon.Utils;

public class Scratchpad
{
    public static string GetTemplate(JArray rules, string route, string departureIcao, string arrivalIcao)
    {
        if (rules == null || route == null) return null;
        string routeNorm = Regex.Replace(route.ToUpperInvariant(), @"\s+", " ").Trim();

        foreach (var r in rules.OfType<JObject>())
        {
            var pattern = r.Value<string>("searchPattern");
            var template = r.Value<string>("template");
            if (string.IsNullOrWhiteSpace(pattern) || template == null) continue;

            if (PatternExists(routeNorm, pattern))
            {
                return template;
            }
        }
        return null;
    }

    private static bool PatternExists(string routeNorm, string pattern)
    {
        string rx = WildcardToRegex(pattern);
        return Regex.IsMatch(routeNorm, rx, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string WildcardToRegex(string pattern)
    {
        string p = Regex.Escape(pattern.ToUpperInvariant());
        p = p.Replace("\\#", "\\d+").Replace("\\*", "\\S*").Replace("\\?", "\\S");
        return @"(?<=^|\s)" + p + @"(?=\s|$)";
    }
}
