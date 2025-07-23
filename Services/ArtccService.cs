using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Interfaces;

namespace vFalcon.Services
{
    public class ArtccService : IArtccService
    {
        public IEnumerable<string> GetArtccs()
        {
            return new[]
            {
                "Albuquerque ARTCC - ZAB",
                "Anchorage ARTCC - ZAN",
                "Atlanta ARTCC - ZTL",
                "Boston ARTCC - ZBW",
                "Chicago ARTCC - ZAU",
                "Cleveland ARTCC - ZOB",
                "Denver ARTCC - ZDV",
                "Fort Worth ARTCC - ZFW",
                "Guam CERAP - ZUA",
                "Honolulu Control Facility - ZHN",
                "Houston ARTCC - ZHU",
                "Indianapolis ARTCC - ZID",
                "Jacksonville ARTCC - ZJX",
                "Kansas City ARTCC - ZKC",
                "Los Angeles ARTCC - ZLA",
                "Memphis ARTCC - ZME",
                "Miami ARTCC - ZMA",
                "Minneapolis ARTCC - ZMP",
                "New York ARTCC - ZNY",
                "Oakland ARTCC - ZOA",
                "Salt Lake ARTCC - ZLC",
                "San Juan CERAP - ZSU",
                "Seattle ARTCC - ZSE",
                "Washington ARTCC - ZDC"
            };
        }

        public IEnumerable<string> GetArtccSectors(string artccId)
        {
            var sectors = new List<string>();
            string json = File.ReadAllText(Loader.LoadFile($"ARTCCs/{artccId}", $"{artccId}.json"));
            var root = JObject.Parse(json);
            var positions = root["positions"] as JArray;

            if (positions == null)
                return Enumerable.Empty<string>();

            foreach (var position in positions)
            {
                string name = position["name"]?.ToString() ?? "Unknown";
                long frequencyHz = position["frequency"]?.ToObject<long>() ?? 0;
                double frequencyMHz = frequencyHz / 1_000_000.0;

                string display = $"{name} - {frequencyMHz:F3}";
                sectors.Add(display);
            }
            return sectors;
        }

        public void BuildArtccFile(string inputPath, string outputPath)
        {
            string jsonText = File.ReadAllText(inputPath);

            JObject sourceJson = JObject.Parse(jsonText);

            JObject summarizedJson = new JObject
            {
                ["id"] = sourceJson["id"],
                ["type"] = sourceJson["facility"]?["type"],
                ["name"] = sourceJson["facility"]?["name"]
            };

            JArray positionsArray = new JArray();

            foreach (var position in sourceJson["facility"]?["positions"] ?? new JArray())
            {
                JObject pos = new JObject
                {
                    ["name"] = position["name"],
                    ["radioName"] = position["radioName"],
                    ["callsign"] = position["callsign"],
                    ["frequency"] = position["frequency"]
                };

                positionsArray.Add(pos);
            }

            summarizedJson["positions"] = positionsArray;

            string outputJson = JsonConvert.SerializeObject(summarizedJson, Formatting.Indented);

            File.WriteAllText(outputPath, outputJson);
        }
    }
}
