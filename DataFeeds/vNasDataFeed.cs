using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.DataFeeds
{
    public class vNasDataFeed
    {
        private static readonly string vNasDataFeedUrl = "https://live.env.vnas.vatsim.net/data-feed/controllers.json";

        public static async Task<string?> GetArtccFrequencyAsync(string artccId, string sectorName)
        {
            using var client = new HttpClient();
            string json = await client.GetStringAsync(vNasDataFeedUrl);

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
