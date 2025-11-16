using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http;
using System.Security.Policy;
using vFalcon.Utils;
namespace vFalcon.DataFeeds;

public class vNasDataFeed
{
    private static readonly HttpClient Http = Client.Create();
    private static readonly string controllerFeedUrl = "https://live.env.vnas.vatsim.net/data-feed/controllers.json";
    public static async Task<JArray> GetArtccNeighboringPositions(string facilityId, JObject facility, CancellationToken ct = default)
    {
        try
        {
            using var resp = await Http.GetAsync(controllerFeedUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            await using var s = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var sr = new System.IO.StreamReader(s);
            using var jr = new Newtonsoft.Json.JsonTextReader(sr) { CloseInput = false };
            var root = await JObject.LoadAsync(jr, ct).ConfigureAwait(false);

            var controllers = root["controllers"] as JArray;
            var neighbors = facility?["neighboringFacilityIds"] as JArray ?? new JArray();
            var childFacilities = facility?["childFacilities"] as JArray ?? new JArray();
            if (controllers is null) return new JArray();

            var internalAirportIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var child in childFacilities.OfType<JObject>())
            {
                var areas = child["starsConfiguration"]?["internalAirports"] as JArray;
                if (areas is null) continue;
                internalAirportIds.Add((string)facility["id"]);
                foreach (var areaTok in areas)
                {
                    string areaId = areaTok.ToString();
                    internalAirportIds.Add(areaId);
                }
            }

            var neighborIds = new HashSet<string>(neighbors.Values<string>(), StringComparer.OrdinalIgnoreCase);

            var allowedIds = new HashSet<string>(neighborIds, StringComparer.OrdinalIgnoreCase);
            allowedIds.UnionWith(internalAirportIds);
            if (!string.IsNullOrEmpty(facilityId)) allowedIds.Add(facilityId);

            var positions = controllers
                .OfType<JObject>()
                .Where(c => !string.Equals((string?)c["role"], "Observer", StringComparison.OrdinalIgnoreCase))
                .SelectMany(c => (c["positions"] as JArray)?.OfType<JObject>() ?? Enumerable.Empty<JObject>())
                .Where(p =>
                {
                    var id = p.Value<string>("facilityId");
                    if (string.IsNullOrEmpty(id)) return false;

                    var isPrimary = p.Value<bool?>("isPrimary") ?? false;

                    var include = isPrimary && allowedIds.Contains(id);
                    return include;
                })
                .ToList();

            var groups = positions.GroupBy(p => new
            {
                Id = p.Value<string>("facilityId") ?? "",
                Name = p.Value<string>("facilityName") ?? "",
            });

            var facilities = new JArray();

            foreach (var g in groups)
            {
                Logger.Debug("F", g.Key.Name);
                var names = new JArray();
                var freqs = new JArray();

                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var p in g)
                {
                    var starsObj = p["starsData"] as JObject;
                    var eramObj = p["eramData"] as JObject;

                    string sectorId = starsObj?.Value<string>("sectorId")
                                     ?? eramObj?.Value<string>("sectorId")
                                     ?? string.Empty;

                    string subset = starsObj?.Value<string>("subset") ?? string.Empty;
                    string posName = p.Value<string>("positionName") ?? string.Empty;

                    string label = $"{subset}{sectorId} {posName}".Trim();

                    string radioName = p.Value<string>("radioName") ?? posName;
                    if (!string.IsNullOrWhiteSpace(sectorId))
                        radioName = $"{sectorId} {posName}".Trim();

                    long hz = p.Value<long?>("frequency") ?? 0L;
                    string mhz = hz > 0 ? (hz / 1_000_000d).ToString("0.000", CultureInfo.InvariantCulture) : string.Empty;

                    string leftText = label;

                    var key = $"{leftText}|{mhz}";

                    if (string.IsNullOrWhiteSpace(leftText) && string.IsNullOrWhiteSpace(mhz))
                        continue;

                    if (seen.Add(key))
                    {
                        names.Add(leftText);
                        freqs.Add(mhz);
                    }
                }

                if (names.Count > 1)
                {
                    var pairs = names.Zip(freqs, (n, f) => new { n, f })
                                     .OrderBy(x => (string)x.n, StringComparer.OrdinalIgnoreCase)
                                     .ToList();
                    names = new JArray(pairs.Select(x => x.n));
                    freqs = new JArray(pairs.Select(x => x.f));
                }

                facilities.Add(new JObject
                {
                    ["FacilityId"] = g.Key.Id,
                    ["FacilityName"] = $"{g.Key.Id} - {g.Key.Name}",
                    ["RadioNames"] = names,
                    ["RadioFreqs"] = freqs
                });
            }

            Logger.Debug("F", facilities.ToString(Newtonsoft.Json.Formatting.Indented));
            return facilities;
        }
        catch (Exception ex)
        {
            Logger.Error("GetArtccNeighboringPositions", ex.ToString());
            return new JArray();
        }
    }
}
