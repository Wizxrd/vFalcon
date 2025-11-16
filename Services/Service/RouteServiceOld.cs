using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using vFalcon.Helpers;

namespace vFalcon.Services.Service
{
    public class RouteServiceOld
    {
        private static readonly string fixPath = Loader.LoadFile("FE-BUDDY\\RawData", "FIX_text.geojson");
        private static readonly string vorPath = Loader.LoadFile("FE-BUDDY\\RawData", "VOR_text.geojson");
        private static readonly string ndbPath = Loader.LoadFile("FE-BUDDY\\RawData", "NDB_text.geojson");
        private static readonly string aptPath = Loader.LoadFile("FE-BUDDY\\RawData", "APT_text.geojson");
        private static readonly string procPath = Loader.LoadFile("FE-BUDDY\\Processed", "Procedures.json");
        private static readonly string airwaysPath = Loader.LoadFile("FE-BUDDY\\Processed", "Airways.json");

        private static readonly Regex _routeSplitRegex = new(@"[\s\.]+", RegexOptions.Compiled);
        private static readonly Regex _airwayRegex = new(@"^[A-Z]{1,2}\d{1,3}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _fixMatchRegex = new(@"^([A-Z]{3,5})(?:/.*)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _procIdRegex = new(@"^[A-Z]{3,5}\d+[A-Z]?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _apt4Regex = new(@"^[A-Z]{4}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Dictionary<string, (double lat, double lon)> FixIndex;
        private static readonly Dictionary<string, (double lat, double lon)> VorIndex;
        private static readonly Dictionary<string, (double lat, double lon)> NdbIndex;
        private static readonly Dictionary<string, (double lat, double lon)> AptIndex;
        private static readonly Dictionary<string, (double lat, double lon)> ProcFirstFixIndex;
        private static readonly Dictionary<(string proc, string fix), (double lat, double lon)> ProcSpecificIndex;
        private static readonly Dictionary<string, List<string>> AirwaysIndex;

        static RouteServiceOld()
        {
            FixIndex = LoadGeoIndex(fixPath);
            VorIndex = LoadGeoIndex(vorPath);
            NdbIndex = LoadGeoIndex(ndbPath);
            AptIndex = LoadGeoIndex(aptPath);
            ProcFirstFixIndex = new(StringComparer.OrdinalIgnoreCase);
            ProcSpecificIndex = new();
            AirwaysIndex = LoadAirwaysIndex(airwaysPath);

            if (!string.IsNullOrWhiteSpace(procPath) && File.Exists(procPath))
            {
                var root = JObject.Parse(File.ReadAllText(procPath));
                foreach (var procProp in root.Properties())
                {
                    var procObj = procProp.Value as JObject;
                    if (procObj == null) continue;
                    (double lat, double lon)? first = null;
                    foreach (var fixProp in procObj.Properties())
                    {
                        var o = fixProp.Value as JObject;
                        if (o?["lat"] == null || o?["lon"] == null) continue;
                        var lat = o["lat"]!.Value<double>();
                        var lon = o["lon"]!.Value<double>();
                        var key = (procProp.Name, fixProp.Name);
                        if (!ProcSpecificIndex.ContainsKey(key))
                            ProcSpecificIndex[key] = (lat, lon);
                        first ??= (lat, lon);
                    }
                    if (first.HasValue && !ProcFirstFixIndex.ContainsKey(procProp.Name))
                        ProcFirstFixIndex[procProp.Name] = first.Value;
                }
            }
        }

        private static Dictionary<string, (double lat, double lon)> LoadGeoIndex(string path)
        {
            var dict = new Dictionary<string, (double lat, double lon)>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return dict;
            var root = JObject.Parse(File.ReadAllText(path));
            var feats = root["features"] as JArray;
            if (feats == null) return dict;
            foreach (var f in feats.OfType<JObject>())
            {
                var texts = f["properties"]?["text"] as JArray;
                var coords = f["geometry"]?["coordinates"] as JArray;
                if (texts == null || coords == null || coords.Count < 2) continue;
                var lon = coords[0]!.Value<double>();
                var lat = coords[1]!.Value<double>();
                foreach (var t in texts)
                {
                    var id = t?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(id)) continue;
                    if (!dict.ContainsKey(id))
                        dict[id] = (lat, lon);
                }
            }
            return dict;
        }

        private static Dictionary<string, List<string>> LoadAirwaysIndex(string path)
        {
            var idx = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return idx;
            var root = JObject.Parse(File.ReadAllText(path));
            foreach (var a in root.Properties())
            {
                if (a.Value is not JObject fixesObj) continue;
                var list = new List<string>();
                foreach (var p in fixesObj.Properties())
                    list.Add(p.Name.ToUpperInvariant());
                if (list.Count > 0) idx[a.Name.ToUpperInvariant()] = list;
            }
            return idx;
        }

        public RouteServiceOld() { }

        public static string GetNextFix(double currentLat, double currentLon, string route, int heading)
        {
            if (string.IsNullOrWhiteSpace(route)) return string.Empty;

            var expandedRoute = ExpandAirwaysInRoute(route);
            var tokens = _routeSplitRegex.Split(expandedRoute);
            var fixPts = new List<(string id, int tok, double lat, double lon)>(tokens.Length);

            for (int i = 0; i < tokens.Length; i++)
            {
                var t = tokens[i];
                if (string.IsNullOrWhiteSpace(t)) continue;
                if (t.Equals("DCT", StringComparison.OrdinalIgnoreCase)) continue;
                if (_airwayRegex.IsMatch(t)) continue;
                var m = _fixMatchRegex.Match(t);
                if (!m.Success) continue;
                var id = m.Groups[1].Value.ToUpperInvariant();
                if (!TryGetCoords(id, out var lat, out var lon)) continue;
                fixPts.Add((id, i, lat, lon));
            }

            if (fixPts.Count == 0) return expandedRoute.Trim();
            if (fixPts.Count == 1) return string.Join(" ", tokens.Skip(fixPts[0].tok));

            double lat0 = ScreenMap.Deg2Rad(currentLat);
            double lon0 = ScreenMap.Deg2Rad(currentLon);
            double hdg = ((heading % 360) + 360) % 360;

            int n = fixPts.Count;
            var xs = new double[n];
            var ys = new double[n];
            for (int i = 0; i < n; i++)
            {
                var la = ScreenMap.Deg2Rad(fixPts[i].lat);
                var lo = ScreenMap.Deg2Rad(fixPts[i].lon);
                xs[i] = (lo - lon0) * Math.Cos(0.5 * (lat0 + la));
                ys[i] = la - lat0;
            }

            var cum = new double[n];
            for (int i = 0; i < n - 1; i++)
            {
                var dx = xs[i + 1] - xs[i];
                var dy = ys[i + 1] - ys[i];
                cum[i + 1] = cum[i] + Math.Sqrt(dx * dx + dy * dy);
            }

            double bestD2 = double.MaxValue;
            int bestSeg = 0;
            double bestT = 0;
            for (int i = 0; i < n - 1; i++)
            {
                var x1 = xs[i]; var y1 = ys[i];
                var x2 = xs[i + 1] - x1; var y2 = ys[i + 1] - y1;
                var vv = x2 * x2 + y2 * y2;
                if (vv <= 0) continue;
                var tRaw = -(x1 * x2 + y1 * y2) / vv;
                var t = tRaw < 0 ? 0 : (tRaw > 1 ? 1 : tRaw);
                var px = x1 + t * x2;
                var py = y1 + t * y2;
                var d2 = px * px + py * py;
                if (d2 < bestD2) { bestD2 = d2; bestSeg = i; bestT = t; }
            }

            var progress = cum[bestSeg] + bestT * (cum[bestSeg + 1] - cum[bestSeg]);
            int startFixIdx = 0;
            while (startFixIdx < n - 1 && cum[startFixIdx] + 1e-9 < progress) startFixIdx++;

            const double coneHalfAngle = 45.0;
            int chosenIdx = -1;
            double bestConeD2 = double.MaxValue;

            for (int i = startFixIdx; i < n; i++)
            {
                double la = ScreenMap.Deg2Rad(fixPts[i].lat);
                double lo = ScreenMap.Deg2Rad(fixPts[i].lon);
                double dlon = lo - lon0;
                double y = Math.Sin(dlon) * Math.Cos(la);
                double x = Math.Cos(lat0) * Math.Sin(la) - Math.Sin(lat0) * Math.Cos(la) * Math.Cos(dlon);
                double brg = Math.Atan2(y, x) * 180.0 / Math.PI;
                if (brg < 0) brg += 360.0;

                double ad = Math.Abs(hdg - brg);
                if (ad > 180) ad = 360 - ad;

                if (ad <= coneHalfAngle)
                {
                    double dx = xs[i];
                    double dy = ys[i];
                    double d2 = dx * dx + dy * dy;
                    if (d2 < bestConeD2) { bestConeD2 = d2; chosenIdx = i; }
                }
            }

            if (chosenIdx < 0) chosenIdx = startFixIdx;

            int startToken = fixPts[chosenIdx].tok;
            return string.Join(" ", tokens.Skip(startToken));
        }

        private static string ExpandAirwaysInRoute(string route)
        {
            var tokens = _routeSplitRegex.Split(route).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            var result = new List<string>();
            int i = 0;

            bool IsPoint(string t) =>
                _fixMatchRegex.IsMatch(t) || _apt4Regex.IsMatch(t) || _procIdRegex.IsMatch(t);

            string ExtractId(string t)
            {
                var m = _fixMatchRegex.Match(t);
                if (m.Success) return m.Groups[1].Value.ToUpperInvariant();
                return t.ToUpperInvariant();
            }

            while (i < tokens.Count)
            {
                var t = tokens[i];

                if (_airwayRegex.IsMatch(t))
                {
                    int li = i - 1;
                    while (li >= 0 && !IsPoint(tokens[li])) li--;
                    int ri = i + 1;
                    while (ri < tokens.Count && !IsPoint(tokens[ri])) ri++;

                    if (li >= 0 && ri < tokens.Count)
                    {
                        var leftId = ExtractId(tokens[li]);
                        var rightId = ExtractId(tokens[ri]);
                        var span = GetAirwayFixes($"{leftId} {t} {rightId}");
                        if (!string.IsNullOrWhiteSpace(span))
                        {
                            if (result.Count > 0)
                            {
                                if (!result[^1].Equals(ExtractId(tokens[li]), StringComparison.OrdinalIgnoreCase))
                                    result.Add(ExtractId(tokens[li]));
                            }
                            var spanTokens = _routeSplitRegex.Split(span).Where(s => !string.IsNullOrWhiteSpace(s));
                            foreach (var s in spanTokens) result.Add(s);
                            i = ri + 1;
                            continue;
                        }
                    }

                    result.Add(t);
                    i++;
                    continue;
                }

                result.Add(t);
                i++;
            }

            return string.Join(" ", result);
        }

        public static string GetAirwayFixes(string airway)
        {
            if (string.IsNullOrWhiteSpace(airway)) return string.Empty;
            var parts = Regex.Split(airway.Trim(), @"\s+");
            if (parts.Length != 3) return string.Empty;

            var start = parts[0].ToUpperInvariant();
            var name = parts[1].ToUpperInvariant();
            var end = parts[2].ToUpperInvariant();

            if (!AirwaysIndex.TryGetValue(name, out var ordered)) return string.Empty;

            int si = ordered.FindIndex(f => f.Equals(start, StringComparison.OrdinalIgnoreCase));
            int ei = ordered.FindIndex(f => f.Equals(end, StringComparison.OrdinalIgnoreCase));
            if (si < 0 || ei < 0) return string.Empty;

            if (si <= ei)
                return string.Join(" ", ordered.Skip(si).Take(ei - si + 1));
            else
            {
                var rev = new List<string>();
                for (int k = si; k >= ei; k--) rev.Add(ordered[k]);
                return string.Join(" ", rev);
            }
        }

        private static bool TryGetCoords(string id, out double lat, out double lon)
        {
            (double lat, double lon) p;
            if (FixIndex.TryGetValue(id, out p) ||
                VorIndex.TryGetValue(id, out p) ||
                NdbIndex.TryGetValue(id, out p) ||
                AptIndex.TryGetValue(id, out p))
            {
                lat = p.lat; lon = p.lon; return true;
            }

            if (_procIdRegex.IsMatch(id) && ProcFirstFixIndex.TryGetValue(id, out p))
            {
                lat = p.lat; lon = p.lon; return true;
            }

            lat = 0; lon = 0; return false;
        }

        public static List<double> GetCoords(string fixName)
        {
            if (TryGetCoords(fixName, out var lat, out var lon))
                return new List<double> { lat, lon };
            return new List<double>();
        }
    }
}
