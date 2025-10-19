using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vFalcon.Helpers;

public static class Parser
{
    public static async Task CreateAirwaysAsync()
    {
        var aliasPath = Loader.LoadFile("FE-BUDDY\\RawData", "AWY_ALIAS.txt");
        var fixPath = Loader.LoadFile("FE-BUDDY\\RawData", "FIX_text.geojson");
        var vorPath = Loader.LoadFile("FE-BUDDY\\RawData", "VOR_text.geojson");
        var ndbPath = Loader.LoadFile("FE-BUDDY\\RawData", "NDB_text.geojson");

        var outPath = Loader.LoadFile("FE-BUDDY\\Processed", "Airways.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

        var fixIndexTask = BuildIndexAsync(fixPath);
        var vorIndexTask = BuildIndexAsync(vorPath);
        var ndbIndexTask = BuildIndexAsync(ndbPath);
        await Task.WhenAll(fixIndexTask, vorIndexTask, ndbIndexTask).ConfigureAwait(false);

        var fixIndex = fixIndexTask.Result;
        var vorIndex = vorIndexTask.Result;
        var ndbIndex = ndbIndexTask.Result;

        var result = new JObject();
        var notFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lines = await File.ReadAllLinesAsync(aliasPath).ConfigureAwait(false);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var tokens = Regex.Split(line.Trim(), @"\s+");
            if (tokens.Length < 2 || !tokens[0].StartsWith(".")) continue;

            var airway = tokens[0].TrimStart('.');
            if (airway.EndsWith("F", StringComparison.OrdinalIgnoreCase)) airway = airway[..^1];

            if (!result.ContainsKey(airway)) result[airway] = new JObject();
            var fixesObj = (JObject)result[airway];

            foreach (var t in tokens.Skip(1))
            {
                if (t.StartsWith(".")) continue;
                var ident = t.Trim().ToUpperInvariant();
                if (ident.Length == 0 || fixesObj.ContainsKey(ident)) continue;

                if (TryResolve(ident, fixIndex, vorIndex, ndbIndex, out var lat, out var lon))
                    fixesObj[ident] = new JObject { ["lat"] = lat, ["lon"] = lon };
                else
                {
                    fixesObj[ident] = new JObject { ["lat"] = null, ["lon"] = null };
                    notFound.Add(ident);
                }
            }
        }

        await File.WriteAllTextAsync(outPath, JsonConvert.SerializeObject(result, Formatting.Indented)).ConfigureAwait(false);
    }

    public static async Task CreateProceduresAsync()
    {
        var aliasPath = Loader.LoadFile("FE-BUDDY\\RawData", "STAR_DP_Fixes_Alias.txt");
        var fixPath = Loader.LoadFile("FE-BUDDY\\RawData", "FIX_text.geojson");
        var vorPath = Loader.LoadFile("FE-BUDDY\\RawData", "VOR_text.geojson");
        var ndbPath = Loader.LoadFile("FE-BUDDY\\RawData", "NDB_text.geojson");

        var outPath = Loader.LoadFile("FE-BUDDY\\Processed", "Procedures.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

        var fixIndexTask = BuildIndexAsync(fixPath);
        var vorIndexTask = BuildIndexAsync(vorPath);
        var ndbIndexTask = BuildIndexAsync(ndbPath);
        await Task.WhenAll(fixIndexTask, vorIndexTask, ndbIndexTask).ConfigureAwait(false);

        var fixIndex = fixIndexTask.Result;
        var vorIndex = vorIndexTask.Result;
        var ndbIndex = ndbIndexTask.Result;

        var result = new JObject();
        var notFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lines = await File.ReadAllLinesAsync(aliasPath).ConfigureAwait(false);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var tokens = Regex.Split(line.Trim(), @"\s+");
            if (tokens.Length < 2 || !tokens[0].StartsWith(".")) continue;

            var m = Regex.Match(tokens[0], @"^\.(?<apt>[A-Za-z]{3})(?<proc>[A-Za-z]{3,5})F$", RegexOptions.CultureInvariant);
            if (!m.Success) continue;

            var proc = m.Groups["proc"].Value.ToUpperInvariant();

            if (!result.ContainsKey(proc)) result[proc] = new JObject();
            var fixesObj = (JObject)result[proc];

            foreach (var t in tokens.Skip(1))
            {
                if (t.StartsWith(".")) continue;
                var ident = t.Trim().ToUpperInvariant();
                if (ident.Length == 0 || fixesObj.ContainsKey(ident)) continue;

                if (TryResolve(ident, fixIndex, vorIndex, ndbIndex, out var lat, out var lon))
                    fixesObj[ident] = new JObject { ["lat"] = lat, ["lon"] = lon };
                else
                {
                    fixesObj[ident] = new JObject { ["lat"] = null, ["lon"] = null };
                    notFound.Add(ident);
                }
            }
        }

        await File.WriteAllTextAsync(outPath, JsonConvert.SerializeObject(result, Formatting.Indented)).ConfigureAwait(false);
    }

    private static bool TryResolve(
        string ident,
        Dictionary<string, (double lat, double lon)> fixIndex,
        Dictionary<string, (double lat, double lon)> vorIndex,
        Dictionary<string, (double lat, double lon)> ndbIndex,
        out double lat, out double lon)
    {
        lat = lon = 0;
        var sources = ident.Length == 3
            ? new[] { vorIndex, ndbIndex, fixIndex }
            : new[] { fixIndex, vorIndex, ndbIndex };

        foreach (var src in sources)
            if (src.TryGetValue(ident, out var p))
            {
                lat = p.lat; lon = p.lon;
                return true;
            }
        return false;
    }

    private static async Task<Dictionary<string, (double lat, double lon)>> BuildIndexAsync(string geojsonPath)
    {
        var dict = new Dictionary<string, (double, double)>(StringComparer.OrdinalIgnoreCase);
        var json = await File.ReadAllTextAsync(geojsonPath).ConfigureAwait(false);
        var root = JObject.Parse(json);
        foreach (var f in root["features"]!.OfType<JObject>())
        {
            var coords = (JArray)f["geometry"]!["coordinates"]!;
            double lon = coords[0]!.Value<double>();
            double lat = coords[1]!.Value<double>();

            var texts = f["properties"]!["text"] as JArray;
            if (texts == null || texts.Count == 0) continue;

            foreach (var t in texts)
            {
                var ident = t!.ToString().Trim().ToUpperInvariant();
                if (ident.Length == 0) continue;
                if (!dict.ContainsKey(ident)) dict[ident] = (lat, lon);
            }
        }
        return dict;
    }
}
