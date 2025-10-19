using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace vFalcon.Helpers
{
    public static class ScratchpadMatcher
    {
        public static string GetTemplate(JArray rules, string route, string departureIcao, string arrivalIcao)
        {
            if (rules == null || route == null) return null;

            string dep = (departureIcao ?? "").Trim().ToUpperInvariant().TrimStart('K');
            string arr = (arrivalIcao ?? "").Trim().ToUpperInvariant().TrimStart('K');
            string routeNorm = Regex.Replace(route.ToUpperInvariant(), @"\s+", " ").Trim();

            foreach (var r in rules.OfType<JObject>())
            {
                var airports = (r["airportIds"] as JArray)?.Select(a => a.ToString().ToUpperInvariant()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? [];
                if (airports.Count > 0 && !(airports.Contains(dep) || airports.Contains(arr))) continue;

                string search = r.Value<string>("searchPattern") ?? "";
                string pattern = BuildRoutePattern(search);
                if (pattern == null) continue;

                if (Regex.IsMatch(routeNorm, pattern, RegexOptions.IgnoreCase))
                    return r.Value<string>("template");
            }

            return null;
        }

        static string BuildRoutePattern(string searchPattern)
        {
            if (string.IsNullOrWhiteSpace(searchPattern)) return null;
            var parts = Regex.Split(searchPattern.Trim().ToUpperInvariant(), @"\s+")
                             .Select(p => Regex.Escape(p).Replace(@"\#", @"\d+"));
            return $@"\b{string.Join(@"\s+", parts)}\b";
        }
    }
}
