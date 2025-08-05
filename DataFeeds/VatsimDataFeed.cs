using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.DataFeeds
{
    public class VatsimDataFeed
    {
        private static readonly string vatsimDataFeedUrl = "https://data.vatsim.net/v3/vatsim-data.json";
        private static string transceiversDataFeed = "https://data.vatsim.net/v3/transceivers-data.json";
        public static async Task<JObject?> AsyncGet()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(vatsimDataFeedUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseData = await response.Content.ReadAsStringAsync();
                        JObject vatsimData = JObject.Parse(responseData);
                        return vatsimData;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<Dictionary<string, string>> LoadTransceiverFrequenciesAsync()
        {
            using var client = new HttpClient();
            string json = await client.GetStringAsync(transceiversDataFeed);

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
    }
}
