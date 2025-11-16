using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace vFalcon.DataFeeds
{
    public static class VatsimDataFeed
    {
        private static readonly HttpClient Http = CreateClient();
        private static HttpClient CreateClient()
        {
            var h = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            var c = new HttpClient(h) { Timeout = TimeSpan.FromSeconds(10) };
            c.DefaultRequestHeaders.UserAgent.ParseAdd("vFalcon/1.0 (+https://vatsim.net)");
            c.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            c.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            return c;
        }

        private static readonly string VatsimUrl = "https://data.vatsim.net/v3/vatsim-data.json";
        private static readonly string TransceiversUrl = "https://data.vatsim.net/v3/transceivers-data.json";

        public static async Task<JObject?> GetPilotsAsync(CancellationToken ct = default)
        {
            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var resp = await Http.GetAsync(VatsimUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                    resp.EnsureSuccessStatusCode();
                    using var s = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                    using var sr = new System.IO.StreamReader(s);
                    using var jr = new JsonTextReader(sr);
                    return await JObject.LoadAsync(jr, ct).ConfigureAwait(false);
                }
                catch (HttpRequestException) when (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct).ConfigureAwait(false);
                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested && attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct).ConfigureAwait(false);
                }
            }
            return null;
        }

        public static async Task<Dictionary<string, string>> GetTransceiversAsync(CancellationToken ct = default)
        {
            using var resp = await Http.GetAsync(TransceiversUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
            using var s = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var sr = new System.IO.StreamReader(s);
            using var jr = new JsonTextReader(sr);
            var arr = await JArray.LoadAsync(jr, ct).ConfigureAwait(false);

            var frequencies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var station in arr)
            {
                var callsign = station?["callsign"]?.ToString();
                if (string.IsNullOrWhiteSpace(callsign)) continue;

                var trx = station["transceivers"] as JArray;
                if (trx is null || trx.Count == 0) continue;

                var first = trx[0]?["frequency"]?.ToObject<long?>();
                if (first is null || first.Value <= 0) continue;

                var mhz = first.Value / 1_000_000.0;
                frequencies[callsign] = mhz.ToString("F3");
            }
            return frequencies;
        }
    }
}
