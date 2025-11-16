using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.DataFeeds
{
    public static class vNasDataFeed
    {
        private static readonly string Url = "https://live.env.vnas.vatsim.net/data-feed/controllers.json";
        private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, string> Cache2 = new(StringComparer.OrdinalIgnoreCase);

        private static readonly HttpClient Http = CreateClient();
        private static HttpClient CreateClient()
        {
            var h = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            var c = new HttpClient(h) { Timeout = TimeSpan.FromSeconds(10) };
            c.DefaultRequestHeaders.UserAgent.ParseAdd("vFalcon/1.0");
            c.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            c.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            return c;
        }

        public static async Task<JArray> GetArtccNeighboringPositions(string facilityId, JObject facility, CancellationToken ct = default)
        {
            try
            {
                using var resp = await Http.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                resp.EnsureSuccessStatusCode();
                await using var s = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                using var sr = new System.IO.StreamReader(s);
                using var jr = new Newtonsoft.Json.JsonTextReader(sr) { CloseInput = false };
                var root = await JObject.LoadAsync(jr, ct).ConfigureAwait(false);

                var controllers = root["controllers"] as JArray;
                var neighbors = facility?["neighboringFacilityIds"] as JArray ?? new JArray();
                var childFacilities = facility?["childFacilities"] as JArray ?? new JArray();
                if (controllers is null) return new JArray();

                // Build internalAirportIds from childFacilities[*].starsConfiguration.areas (string or object)
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

                // Neighbor IDs (your sample shows plain strings)
                var neighborIds = new HashSet<string>(neighbors.Values<string>(), StringComparer.OrdinalIgnoreCase);

                // Combine: neighbors ∪ internal airports ∪ (optional) self facilityId
                var allowedIds = new HashSet<string>(neighborIds, StringComparer.OrdinalIgnoreCase);
                allowedIds.UnionWith(internalAirportIds);
                if (!string.IsNullOrEmpty(facilityId)) allowedIds.Add(facilityId);

                // Pull positions (skip Observer controllers)
                var positions = controllers
                    .OfType<JObject>()
                    .Where(c => !string.Equals((string?)c["role"], "Observer", StringComparison.OrdinalIgnoreCase))
                    .SelectMany(c => (c["positions"] as JArray)?.OfType<JObject>() ?? Enumerable.Empty<JObject>())
                    .Where(p =>
                    {
                        var id = p.Value<string>("facilityId");  // <-- you missed ';' earlier
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

                    // de-dup by (RadioName, MHz) pair (case-insensitive)
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

                        // display label
                        string label = $"{subset}{sectorId} {posName}".Trim();

                        // radio name fallback (if you prefer label in the left column, keep using label)
                        string radioName = p.Value<string>("radioName") ?? posName;
                        if (!string.IsNullOrWhiteSpace(sectorId))
                            radioName = $"{sectorId} {posName}".Trim();

                        // frequency -> MHz (string)
                        long hz = p.Value<long?>("frequency") ?? 0L;
                        string mhz = hz > 0 ? (hz / 1_000_000d).ToString("0.000", CultureInfo.InvariantCulture) : string.Empty;

                        // choose which text goes in the "names" column:
                        // - if you want the label with subset/sector/position:
                        string leftText = label;
                        // - if instead you want the actual radio name, use:
                        // string leftText = radioName;

                        // build de-dupe key; change to positionId if that's your uniqueness rule:
                        // var key = p.Value<string>("positionId") ?? $"{leftText}|{mhz}";
                        var key = $"{leftText}|{mhz}";

                        if (string.IsNullOrWhiteSpace(leftText) && string.IsNullOrWhiteSpace(mhz))
                            continue; // skip empty rows

                        if (seen.Add(key))
                        {
                            names.Add(leftText);
                            freqs.Add(mhz);
                        }
                    }

                    // (optional) keep rows aligned & sorted together by left column
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

        public static async Task<string?> GetArtccFrequencyAsync(string artccId, string sectorName, CancellationToken ct = default)
        {
            var cacheKey = artccId + "||" + sectorName;
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var resp = await Http.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                    resp.EnsureSuccessStatusCode();
                    await using var s = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                    using var sr = new System.IO.StreamReader(s);
                    using var jr = new Newtonsoft.Json.JsonTextReader(sr) { CloseInput = false };
                    var root = await JObject.LoadAsync(jr, ct).ConfigureAwait(false);

                    var controllers = root["controllers"] as JArray;
                    if (controllers is null) return Cache.TryGetValue(cacheKey, out var freq0) ? freq0 : null;

                    foreach (var controller in controllers)
                    {
                        var id = controller?["artccId"]?.ToString();
                        if (!string.Equals(id, artccId, StringComparison.OrdinalIgnoreCase)) continue;

                        var positions = controller?["positions"] as JArray;
                        if (positions is null) continue;

                        foreach (var position in positions)
                        {
                            var name = position?["positionName"]?.ToString();
                            if (!string.Equals(name, sectorName, StringComparison.OrdinalIgnoreCase)) continue;

                            var hz = position?["frequency"]?.ToObject<long?>() ?? 0;
                            if (hz > 0)
                            {
                                var mhz = (hz / 1_000_000.0).ToString("F3");
                                Cache[cacheKey] = mhz;
                                return mhz;
                            }
                        }
                    }

                    return Cache.TryGetValue(cacheKey, out var freq) ? freq : null;
                }
                catch (HttpRequestException) when (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested && attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct).ConfigureAwait(false);
                }
                catch (Newtonsoft.Json.JsonReaderException) when (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct).ConfigureAwait(false);
                }
            }

            return Cache.TryGetValue(cacheKey, out var fallback) ? fallback : null;
        }
    }
}
