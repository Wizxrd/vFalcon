using System.IO;
using Newtonsoft.Json.Linq;
using vFalcon.Models;

namespace vFalcon.Helpers
{
    public class ArtccBox
    {
        public string Id { get; set; }
        public double MinLat { get; set; }
        public double MaxLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLon { get; set; }

        public bool Contains(double lat, double lon)
        {
            return lat >= MinLat && lat <= MaxLat &&
                   lon >= MinLon && lon <= MaxLon;
        }

        public Coordinate GetCenter()
        {
            return new Coordinate(
                (MinLat + MaxLat) / 2,
                (MinLon + MaxLon) / 2
            );
        }

        public static ArtccBox? Load(string artccId)
        {
            string jsonText = File.ReadAllText(Loader.LoadFile($"VideoMaps/{artccId}", "BOUNDARY.geojson"));
            var featureCollection = JObject.Parse(jsonText);
            foreach (var feature in featureCollection["features"])
            {
                string id = feature["properties"]?["id"]?.ToString() ?? "UNKNOWN";
                if (!id.Equals(artccId, StringComparison.OrdinalIgnoreCase))
                    continue;
                JArray? bboxArray = feature["bbox"] as JArray ?? feature["geometry"]?["bbox"] as JArray;
                if (bboxArray != null && bboxArray.Count == 4)
                {
                    return new ArtccBox
                    {
                        Id = id,
                        MinLon = bboxArray[0].ToObject<double>(),
                        MinLat = bboxArray[1].ToObject<double>(),
                        MaxLon = bboxArray[2].ToObject<double>(),
                        MaxLat = bboxArray[3].ToObject<double>()
                    };
                }
                break;
            }
            return null;
        }
    }
}
