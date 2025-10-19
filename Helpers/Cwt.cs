using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Helpers
{
    public class Cwt
    {
        private static string path = Path.Combine(Loader.GetAppDirectory(), "AircraftCwt.json");
        private static string json = File.ReadAllText(path);
        private static JArray jArray = JArray.Parse(json);

        public static string GetCwtCodeFromType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return string.Empty;

            var match = jArray
                .FirstOrDefault(o => string.Equals(
                    (string?)o["typeCode"],
                    type,
                    StringComparison.OrdinalIgnoreCase));

            return match?["cwtCode"]?.Value<string>() ?? string.Empty;
        }

    }
}
