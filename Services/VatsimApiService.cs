using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Services
{
    public class VatsimApiService
    {
        public static async Task<Dictionary<string, string>> LoadTransceiverFrequenciesAsync()
        {
            string url = "https://data.vatsim.net/v3/transceivers-data.json";
            using var client = new HttpClient();
            string json = await client.GetStringAsync(url);

            var transceiversData = JArray.Parse(json);
            var frequencies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var station in transceiversData)
            {
                string callsign = station["callsign"]?.ToString() ?? "";
                var transceivers = station["transceivers"] as JArray;

                if (!string.IsNullOrEmpty(callsign) && transceivers != null && transceivers.Count > 0)
                {
                    var frequencyHz = (long)transceivers[0]["frequency"];
                    double frequencyMHz = frequencyHz / 1_000_000.0;
                    frequencies[callsign] = frequencyMHz.ToString("F3");
                }
            }

            return frequencies;
        }

        public static async Task<string?> GetArtccFrequencyAsync(string artccId, string sectorName)
        {
            string url = "https://live.env.vnas.vatsim.net/data-feed/controllers.json";
            using var client = new HttpClient();
            string json = await client.GetStringAsync(url);

            var root = JObject.Parse(json);
            var controllers = root["controllers"] as JArray;

            foreach (var controller in controllers)
            {
                string controllerArtccId = controller["artccId"]?.ToString() ?? "";
                if (!controllerArtccId.Equals(artccId, StringComparison.OrdinalIgnoreCase))
                    continue;

                var positions = controller["positions"] as JArray;
                if (positions == null) continue;

                foreach (var position in positions)
                {
                    string facilitySectorName = position["positionName"]?.ToString() ?? "";
                    if (!facilitySectorName.Equals(sectorName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    long frequencyHz = position["frequency"]?.ToObject<long>() ?? 0;
                    if (frequencyHz > 0)
                    {
                        double frequencyMHz = frequencyHz / 1_000_000.0;
                        return frequencyMHz.ToString("F3");
                    }
                }
            }

            return null;
        }
    }
}
