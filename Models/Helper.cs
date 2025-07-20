using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace vFalcon.Models
{
    class Helper
    {
        public static string GenerateUniqueHash()
        {
            return Guid.NewGuid().ToString();
        }

        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject? parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T correctlyTyped)
                    return correctlyTyped;

                T? result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static JObject ConvertLineStringsToPolygon(string sourceFile, string artccId)
        {
            var allCoordinates = new List<List<double>>();

            string jsonText = File.ReadAllText(sourceFile);
            var featureCollection = JObject.Parse(jsonText);

            foreach (var feature in featureCollection["features"]!)
            {
                var geometryType = feature["geometry"]?["type"]?.ToString();
                if (geometryType == "LineString")
                {
                    var coordinatesArray = feature["geometry"]?["coordinates"]?.ToObject<List<List<double>>>();
                    if (coordinatesArray != null)
                    {
                        foreach (var coord in coordinatesArray)
                        {
                            if (coord.Count >= 2)
                            {
                                allCoordinates.Add(new List<double> { coord[0], coord[1] });  // [lon, lat]
                            }
                        }
                    }
                }
            }

            var polygonFeature = new JObject
            {
                ["type"] = "Feature",
                ["properties"] = new JObject { ["id"] = artccId },
                ["geometry"] = new JObject
                {
                    ["type"] = "Polygon",
                    ["coordinates"] = new JArray { JArray.FromObject(allCoordinates) }
                }
            };

            var outputGeoJson = new JObject
            {
                ["type"] = "FeatureCollection",
                ["name"] = "ARTCC Boundaries",
                ["crs"] = JObject.Parse(@"{ 'type':'name','properties':{ 'name':'urn:ogc:def:crs:OGC:1.3:CRS84' } }"),
                ["features"] = new JArray { polygonFeature }
            };

            return outputGeoJson;
        }

        public static void BuildArtccFile(string inputPath, string outputPath)
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

        public static void CombineGeoJsonFiles(string[] inputFilePaths, string outputFilePath)
        {
            JArray combinedFeatures = new JArray();

            foreach (string filePath in inputFilePaths)
            {
                string jsonText = File.ReadAllText(filePath);
                JObject geoJson = JObject.Parse(jsonText);

                JArray? features = geoJson["features"] as JArray;
                if (features != null)
                {
                    foreach (var feature in features)
                    {
                        combinedFeatures.Add(feature);
                    }
                }
            }

            JObject outputGeoJson = new JObject
            {
                ["type"] = "FeatureCollection",
                ["features"] = combinedFeatures
            };

            File.WriteAllText(outputFilePath, outputGeoJson.ToString(Newtonsoft.Json.Formatting.Indented));
        }

        public static void CleanGeoJson(string inputFile, string outputFile)
        {
            string jsonText = File.ReadAllText(inputFile);
            var featureCollection = JObject.Parse(jsonText);

            JArray features = (JArray)featureCollection["features"];
            JArray cleanedFeatures = new JArray();

            foreach (var feature in features)
            {
                string geometryType = feature["geometry"]?["type"]?.ToString();

                if (geometryType == "LineString")
                {
                    cleanedFeatures.Add(feature);
                }
            }

            featureCollection["features"] = cleanedFeatures;

            File.WriteAllText(outputFile, featureCollection.ToString());
        }
    }
}
