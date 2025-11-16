using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using vFalcon.Utils;
namespace vFalcon.DataFeeds;

public class VatsimDataFeed
{
    private static readonly string dataFeedUrl = "https://data.vatsim.net/v3/vatsim-data.json";
    private static readonly string trasceiversFeedUrl = "https://data.vatsim.net/v3/transceivers-data.json";
    private static readonly HttpClient Http = Client.Create();

    public static async Task<JObject?> GetDataFeed(CancellationToken ct = default)
    {
        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                using var resp = await Http.GetAsync(dataFeedUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
                resp.EnsureSuccessStatusCode();
                using var stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                using var streamReader = new StreamReader(stream, Encoding.UTF8, true, 16384, leaveOpen: true);
                using var jsonReader = new JsonTextReader(streamReader) { CloseInput = false };
                return JObject.Load(jsonReader);
            }
            catch (HttpRequestException) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct).ConfigureAwait(false);
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested && attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), ct).ConfigureAwait(false);
            }
            catch (ExecutionEngineException ex)
            {
                Logger.Error("VatsimDataFeed.GetDataFeed", ex.ToString());
            }
        }
        return null;
    }

    public static async Task<Dictionary<string, string>> GetTransceiversAsync(CancellationToken ct = default)
    {
        using var resp = await Http.GetAsync(trasceiversFeedUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
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
