using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace vFalcon.DataFeed
{
    public class VatsimDataFeed
    {
        private static readonly string dataFeedURL = "https://data.vatsim.net/v3/vatsim-data.json";
        public static async Task<JObject?> AsyncGet()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(dataFeedURL);

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
    }
}
