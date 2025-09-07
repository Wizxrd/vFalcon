using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace vFalcon.DataFeeds
{
    public static class vNasDataFeed
    {
        private static readonly string Url = "https://live.env.vnas.vatsim.net/data-feed/controllers.json";
        private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

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
