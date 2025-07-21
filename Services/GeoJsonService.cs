using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Services.Interfaces;

namespace vFalcon.Services
{
    public class GeoJsonService : IGeoJsonService
    {
        public JObject ConvertLineStringsToPolygon(string sourceFile, string artccId)
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

        public void CombineGeoJsonFiles(string[] inputFilePaths, string outputFilePath)
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

        public void CleanGeoJson(string inputFile, string outputFile)
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
