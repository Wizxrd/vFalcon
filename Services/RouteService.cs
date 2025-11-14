using CsvHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vFalcon.Utils;
using vFalcon.Models;
using Newtonsoft.Json;

namespace vFalcon.Services;

public class RouteService
{
    private static readonly string airportRegexPattern = @"(?<![A-Z0-9])[A-Z]{4}(?![A-Z0-9])";
    private static readonly string airwayRegexPattern = @"^[A-Z]{1,2}\d{1,3}$";
    private static readonly string fixRegexPattern = @"^(?:[A-Z]{3}|[A-Z]{5})$";
    private static readonly string fixLatLonRegexPattern = @"\b[A-Z0-9]{5}/";
    private static readonly string procedureRegexPattern = @"(?<![A-Z0-9])[A-Z]{3,5}\d(?![A-Z0-9])";

    private static string navDataSerial = PathFinder.GetFilePath("", "NavDataSerial.json");
    private static JObject navData = JObject.Parse(File.ReadAllText(navDataSerial));

    private static string airportCsvPath;
    private static string airwayCsvPath;
    private static string departureCsvPath;
    private static string fixCsvPath;
    private static string navAidCsvPath;
    private static string starCsvPath;

    private static JObject airportLookup;
    private static JObject airwayLookup;
    private static JObject fixLookup;
    private static JObject procedureLookup;

    public RouteService(string? date)
    {
        date = date ?? (string)navData["Date"];
        airportCsvPath = PathFinder.GetFilePath($"NavData\\{date}", "Airports.csv");
        airwayCsvPath = PathFinder.GetFilePath($"NavData\\{date}", "Airways.csv");
        departureCsvPath = PathFinder.GetFilePath($"NavData\\{date}", "Departures.csv");
        fixCsvPath = PathFinder.GetFilePath($"NavData\\{date}", "Fixes.csv");
        navAidCsvPath = PathFinder.GetFilePath($"NavData\\{date}", "NavAids.csv");
        starCsvPath = PathFinder.GetFilePath($"NavData\\{date}", "Stars.csv");

        airportLookup = CreateAirportLookup();
        airwayLookup = CreateAirwayLookup();
        fixLookup = CreateFixLookup();
        procedureLookup = CreateProcedureLookup();
    }

    public string GetNextFix(string route, int heading, double currentLat, double currentLon)
    {
        var fixes = GetFixesFromRouteString(route);
        if (fixes == null || fixes.Count == 0) return string.Empty;

        double h = ScreenMap.NormalizeAngle(heading);
        string best = string.Empty;
        double bestDist = double.PositiveInfinity;

        foreach (var fix in fixes)
        {
            var c = GetFixCoords(fix);
            if (c == null || c.Count < 2) continue;

            double lat = c[0], lon = c[1];
            double brg = ScreenMap.BearingTo(currentLat, currentLon, lat, lon);
            double ang = Math.Abs(ScreenMap.AngleDelta(h, brg));
            if (ang > 90.0) continue;

            double dist = ScreenMap.DistanceInNM(new Coordinate { Lat = currentLat, Lon = currentLon}, new Coordinate { Lat = lat, Lon = lon });
            if (dist < bestDist)
            {
                bestDist = dist;
                best = fix;
            }
        }
        if (best == string.Empty) return fixes[fixes.Count - 1];
        return best;
    }

    public string GetRouteAfterFix(string route, string nextFix)
    {
        if (!string.IsNullOrEmpty(nextFix) && route.Contains(nextFix))
        {
            int index = route.IndexOf(nextFix, StringComparison.OrdinalIgnoreCase);
            if (index >= 0) route = route.Substring(index);
        }
        return route;
    }

    public List<string> GetFixesFromRouteString(string route)
    {
        var fixes = new List<string>();
        if (string.IsNullOrWhiteSpace(route)) return fixes;
        var splitRoute = Regex.Split(route.Trim(), @"\s+")
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .ToArray();
        bool TryGet(int index, out string value)
        {
            if (index >= 0 && index < splitRoute.Length)
            {
                value = splitRoute[index];
                return true;
            }
            value = string.Empty;
            return false;
        }

        bool Is(string s, string pattern) => !string.IsNullOrEmpty(s) && Regex.IsMatch(s, pattern);

        for (int i = 0; i < splitRoute.Length; i++)
        {
            var fix = splitRoute[i];

            if (fix == "DCT") continue;

            if (Is(fix, airportRegexPattern))
            {
                fixes.Add(fix);
                continue;
            }

            if (Is(fix, fixRegexPattern))
            {
                fixes.Add(fix);
                continue;
            }

            if (Is(fix, fixLatLonRegexPattern))
            {
                var cleaned = Regex.Replace(fix, @"/.*$", "");
                fixes.Add(cleaned);
                continue;
            }

            if (Is(fix, airwayRegexPattern))
            {
                if (TryGet(i - 1, out var start) && TryGet(i + 1, out var end))
                {
                    if (Is(start, fixRegexPattern) && Is(end, fixRegexPattern))
                    {
                        var airwayFixes = GetAirwayFixes(start, fix, end);
                        if (airwayFixes != null)
                            fixes.AddRange(airwayFixes);
                    }
                }
                continue;
            }

            if (Is(fix, procedureRegexPattern))
            {
                if (TryGet(i + 1, out var nextTok) && Is(nextTok, fixRegexPattern))
                {
                    var procedureFixes = GetProcedureFixes($"{fix} {nextTok}");
                    if (procedureFixes != null) fixes.AddRange(procedureFixes);
                    continue;
                }

                if (TryGet(i - 1, out var prevTok) && Is(prevTok, fixRegexPattern))
                {
                    var procedureFixes = GetProcedureFixes($"{prevTok} {fix}");
                    if (procedureFixes != null) fixes.AddRange(procedureFixes);
                    continue;
                }
                continue;
            }
        }

        return fixes;
    }

    public JArray GetCoordsFromRouteString(string route)
    {
        var fixes = Regex.Split($"{route}", @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        JArray coords = new JArray();
        for (int i = 0; i < fixes.Length; i++)
        {
            var fix = fixes[i];
            if (fix == "DCT") continue;
            if (Regex.IsMatch(fix, airportRegexPattern))
            {
                var airportCoords = GetAirportCoords(fix);
                if (airportCoords == null) continue;
                coords.Add(new JArray(airportCoords.Lat, airportCoords.Lon));
            }
            else if (Regex.IsMatch(fix, fixRegexPattern))
            {
                var fixCoords = GetFixCoords(fix);
                if (fixCoords == null) continue;
                coords.Add(new JArray(fixCoords[0], fixCoords[1]));
            }
            else if (Regex.IsMatch(fix, fixLatLonRegexPattern))
            {
                fix = Regex.Replace(fix, @"/.*$", "");
                var fixCoords = GetFixCoords(fix);
                if (fixCoords == null) continue;
                coords.Add(new JArray(fixCoords[0], fixCoords[1]));
            }
            else if (Regex.IsMatch(fix, airwayRegexPattern))
            {
                string start = fixes[i - 1];
                string end = fixes[i + 1];
                var airwayCoords = GetAirwayCoords(start, fix, end);
                if (airwayCoords == null) continue;
                foreach (var latlon in airwayCoords)
                {
                    double lat = (double)latlon[0];
                    double lon = (double)latlon[1];
                    coords.Add(new JArray(lat, lon));
                }
            }
            else if (Regex.IsMatch(fix, procedureRegexPattern))
            {
                if (Regex.IsMatch(fixes[i + 1], fixRegexPattern))
                {
                    string transition = fixes[i + 1];
                    var procedureCoords = GetProcedureCoords($"{fix} {transition}");
                    if (procedureCoords == null) continue;
                    foreach (var latlon in procedureCoords)
                    {
                        double lat = (double)latlon[0];
                        double lon = (double)latlon[1];
                        coords.Add(new JArray(lat, lon));
                    }
                }
                else if (Regex.IsMatch(fixes[i - 1], fixRegexPattern))
                {
                    string transition = fixes[i - 1];
                    var procedureCoords = GetProcedureCoords($"{transition} {fix}");
                    if (procedureCoords == null) continue;
                    foreach (var latlon in procedureCoords)
                    {
                        double lat = (double)latlon[0];
                        double lon = (double)latlon[1];
                        coords.Add(new JArray(lat, lon));
                    }
                }
            }
            else
            {
                Logger.Alert("Cannot Match", fix);
            }
        }

        return coords;
    }

    public Coordinate? GetAirportCoords(string airport)
    {
        if (string.IsNullOrWhiteSpace(airport)) return null;
        if (airport.Length == 4 && (airport[0] == 'K' || airport[0] == 'P')) airport = airport.Substring(1);
        List<double> coords = new List<double>();
        if (airportLookup[airport] == null) return null;
        return new Coordinate
        {
            Lat = (double)airportLookup[airport]["Lat"],
            Lon = (double)airportLookup[airport]["Lon"]
        };
    }

    public List<string> GetAirwayFixes(string start, string airway, string end)
    {
        List<string> fixes = new List<string>();
        if (airwayLookup[airway] == null) return null;
        string airwayString = (string)airwayLookup[airway];
        string[] fixesOnAirway = Regex.Split($"{airwayString}", @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        int s = Array.IndexOf(fixesOnAirway, start);
        int e = Array.IndexOf(fixesOnAirway, end);
        if (s == -1 || e == -1) return fixes;
        int step = s <= e ? 1 : -1;
        for (int i = s; i != e + step; i += step)
        {
            var fix = fixesOnAirway[i];
            if (fix == start || fix == end) continue;
            fixes.Add(fix);

        }
        return fixes;
    }

    public JArray? GetAirwayCoords(string start, string airway, string end)
    {
        JArray coords = new JArray();
        if (airwayLookup[airway] == null) return null;
        string airwayString = (string)airwayLookup[airway];
        string[] fixesOnAirway = Regex.Split($"{airwayString}", @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        int s = Array.IndexOf(fixesOnAirway, start);
        int e = Array.IndexOf(fixesOnAirway, end);
        if (s == -1 || e == -1) return coords;
        int step = s <= e ? 1 : -1;
        for (int i = s; i != e + step; i += step)
        {
            var fix = fixesOnAirway[i];
            if (fix == start || fix == end) continue;
            var fixCoords = GetFixCoords(fix);
            coords.Add(new JArray(fixCoords[0], fixCoords[1]));
        }
        return coords;
    }

    public JArray? GetProcedureCoords(string procedure)
    {
        JArray coords = new JArray();
        if (procedureLookup[procedure] == null) return null;
        string procedureRouteString = (string)procedureLookup[procedure];
        string[] fixesOnProcedure = Regex.Split($"{procedureRouteString}", @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        bool sid = false;
        bool star = false;

        string[] procedureSplit = Regex.Split($"{procedure}", @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (Regex.IsMatch(procedureSplit[0], procedureRegexPattern)) sid = true;
        else if (Regex.IsMatch(procedureSplit[1], procedureRegexPattern)) star = true;
        for (int i = 0; i < fixesOnProcedure.Length; i++)
        {
            string fix = fixesOnProcedure[i];
            if (sid && i == fixesOnProcedure.Length - 1) continue;
            else if (star && i == 0) continue;
            var fixCoords = GetFixCoords(fix);
            coords.Add(new JArray(fixCoords[0], fixCoords[1]));
        }
        return coords;
    }

    public List<string>? GetProcedureFixes(string procedure)
    {
        List<string> fixes = new List<string>();
        if (procedureLookup[procedure] == null) return null;
        string procedureRouteString = (string)procedureLookup[procedure];
        string[] fixesOnProcedure = Regex.Split($"{procedureRouteString}", @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        bool sid = false;
        bool star = false;
        string[] procedureSplit = Regex.Split($"{procedure}", @"\s+").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        if (Regex.IsMatch(procedureSplit[0], procedureRegexPattern)) sid = true;
        else if (Regex.IsMatch(procedureSplit[1], procedureRegexPattern)) star = true;
        for (int i = 0; i < fixesOnProcedure.Length; i++)
        {
            string fix = fixesOnProcedure[i];
            if (sid && i == fixesOnProcedure.Length - 1) continue;
            else if (star && i == 0) continue;
            fixes.Add(fix);
        }
        return fixes;
    }

    public List<double>? GetFixCoords(string fix)
    {
        List<double> coords = new List<double>();
        if (fixLookup[fix] == null) return null;
        coords.Add((double)fixLookup[fix]["Lat"]);
        coords.Add((double)fixLookup[fix]["Lon"]);
        return coords;
    }

    private static JObject CreateAirportLookup()
    {
        var airports = new JObject();

        using var sr = new StreamReader(airportCsvPath);
        using var csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var id = (csv.GetField("ArptId") ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(id)) continue;

            var lat = csv.GetField<double>("Lat");
            var lon = csv.GetField<double>("Lon");

            airports[id] = new JObject
            {
                ["Lat"] = lat,
                ["Lon"] = lon
            };
        }

        return airports;
    }

    private static JObject CreateAirwayLookup()
    {
        var airways = new JObject();

        using var sr = new StreamReader(airwayCsvPath);
        using var csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        while (csv.Read())
        {
            var id = (csv.GetField("AwyId") ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(id)) continue;

            var airwayString = csv.GetField<string>("AirwayString");

            airways[id] = airwayString;
        }

        return airways;
    }

    private static JObject CreateFixLookup()
    {
        var fixes = new JObject();
        using var fixSr = new StreamReader(fixCsvPath);
        using var fixCsv = new CsvReader(fixSr, System.Globalization.CultureInfo.InvariantCulture);
        using var navAidSr = new StreamReader(navAidCsvPath);
        using var navAidCsv = new CsvReader(navAidSr, System.Globalization.CultureInfo.InvariantCulture);

        fixCsv.Read();
        fixCsv.ReadHeader();
        while (fixCsv.Read())
        {
            var id = (fixCsv.GetField("FixId") ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(id)) continue;

            var lat = fixCsv.GetField<double>("Lat");
            var lon = fixCsv.GetField<double>("Lon");
            fixes[id] = new JObject
            {
                ["Lat"] = lat,
                ["Lon"] = lon
            };
        }

        navAidCsv.Read();
        navAidCsv.ReadHeader();
        while (navAidCsv.Read())
        {
            var id = (navAidCsv.GetField("NavId") ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(id)) continue;
            var lat = navAidCsv.GetField<double>("Lat");
            var lon = navAidCsv.GetField<double>("Lon");
            fixes[id] = new JObject
            {
                ["Lat"] = lat,
                ["Lon"] = lon
            };
        }

        return fixes;
    }

    private static JObject CreateProcedureLookup()
    {
        JObject procedures = new JObject();
        using var starSr = new StreamReader(starCsvPath);
        using var starCsv = new CsvReader(starSr, System.Globalization.CultureInfo.InvariantCulture);
        using var depSr = new StreamReader(departureCsvPath);
        using var depCsv = new CsvReader(depSr, System.Globalization.CultureInfo.InvariantCulture);

        starCsv.Read();
        starCsv.ReadHeader();
        while (starCsv.Read())
        {
            var id = (starCsv.GetField("TransitionComputerCode") ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(id)) continue;
            var routeString = starCsv.GetField<string>("RouteString");
            procedures[id] = routeString;
        }

        depCsv.Read();
        depCsv.ReadHeader();
        while (depCsv.Read())
        {
            var id = (depCsv.GetField("TransitionComputerCode") ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(id)) continue;
            var routeString = depCsv.GetField<string>("RouteString");
            procedures[id] = routeString;
        }

        return procedures;
    }
}
