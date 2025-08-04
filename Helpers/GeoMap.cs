using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using vFalcon.Models;

namespace vFalcon.Helpers
{
    public class GeoMap
    {
        public static async Task<Dictionary<int, List<ProcessedFeature>>> LoadFacilityFeatures(Artcc artcc, JArray activeVideoMapIds)
        {
            Logger.Info("GeoMap.LoadFacilityFeatures", "Creating FacilityFeatures...");
            string sourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", $"VideoMaps/{artcc.id}");

            var features = new Dictionary<int, List<ProcessedFeature>>();
            var videoMapTdmLookup = new Dictionary<string, bool>();

            foreach (JObject vm in (JArray)artcc.videoMaps)
            {
                string id = vm["id"]?.ToString();
                if (id != null && activeVideoMapIds.Any(aid => aid.ToString() == id))
                {
                    videoMapTdmLookup[id] = vm["tdmOnly"]?.Value<bool>() ?? false;
                }
            }

            var matchedFiles = activeVideoMapIds.Select(id => Path.Combine(sourcePath, id + ".geojson")).Where(File.Exists).ToList();

            await Task.Run(() =>
            {
                try
                {
                    var rawFeatures = new Dictionary<int, List<ProcessedFeature>>();

                    foreach (var file in matchedFiles)
                    {
                        string fileId = Path.GetFileNameWithoutExtension(file);
                        bool tdmOnly = videoMapTdmLookup.TryGetValue(fileId, out var tdm) && tdm;

                        JObject geoJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(file));
                        if (geoJson == null) continue;

                        var defaults = ExtractLastIsDefaults(geoJson);

                        foreach (JObject feature in geoJson["features"])
                        {
                            var properties = feature["properties"] as JObject;
                            if (properties == null || IsDefaultsFeature(properties)) continue;

                            var geomObj = feature["geometry"] as JObject;
                            if (geomObj == null)
                            {
                                Logger.Error("GeoMap.LoadVideoMap", feature["properties"].ToString());
                                continue;
                            }

                            string geometryType = geomObj.Value<string>("type") ?? "Unknown";
                            var finalProperties = MergeProperties(properties, defaults, geometryType);
                            finalProperties["tdmOnly"] = tdmOnly;

                            List<int> filters = new List<int>();
                            if (finalProperties.TryGetValue("filters", out var filtersObj) && filtersObj != null)
                            {
                                if (filtersObj is JArray arr)
                                    filters.AddRange(arr.Where(f => f != null && f.Type != JTokenType.Null).Select(f => (int)f));
                                else if (filtersObj is JValue val && val.Value != null)
                                    filters.Add(val.Value<int>());
                                else if (filtersObj is int intFilter)
                                    filters.Add(intFilter);
                                else if (int.TryParse(filtersObj.ToString(), out int parsed))
                                    filters.Add(parsed);
                            }

                            foreach (int filter in filters)
                            {
                                if (!rawFeatures.ContainsKey(filter))
                                    rawFeatures[filter] = new List<ProcessedFeature>();

                                rawFeatures[filter].Add(new ProcessedFeature
                                {
                                    GeometryType = ResolveFeatureType(geometryType, properties),
                                    Geometry = feature["geometry"],
                                    AppliedAttributes = finalProperties
                                });
                            }
                        }
                    }

                    foreach (var kvp in rawFeatures)
                    {
                        int filterIndex = kvp.Key;
                        var featuresList = kvp.Value;
                        var condensedList = new List<ProcessedFeature>();

                        // Combine lines
                        foreach (var group in featuresList
                            .Where(f => f.GeometryType is "Line" or "LineString" or "MultiLineString")
                            .GroupBy(f => JsonConvert.SerializeObject(f.AppliedAttributes)))
                        {
                            var mergedCoords = new JArray();

                            foreach (var f in group)
                            {
                                var geomObj = f.Geometry as JObject ?? (f.Geometry as JToken as JObject);
                                if (geomObj == null) continue;

                                var coords = geomObj["coordinates"] as JArray;
                                if (coords == null || coords.Count == 0) continue;

                                if (f.GeometryType is "Line" or "LineString")
                                {
                                    mergedCoords.Add(coords);
                                }
                                else if (f.GeometryType == "MultiLineString")
                                {
                                    foreach (var segment in coords.OfType<JArray>())
                                        mergedCoords.Add(segment);
                                }
                            }

                            if (mergedCoords.Count > 0)
                            {
                                condensedList.Add(new ProcessedFeature
                                {
                                    GeometryType = "MultiLineString",
                                    Geometry = new JObject
                                    {
                                        ["type"] = "MultiLineString",
                                        ["coordinates"] = mergedCoords
                                    },
                                    AppliedAttributes = JsonConvert.DeserializeObject<Dictionary<string, object>>(group.Key)
                                });
                            }
                        }

                        // Combine points
                        foreach (var group in featuresList
                            .Where(f => f.GeometryType == "Symbol" && ((f.Geometry as JObject)?["type"]?.ToString() == "Point"))
                            .GroupBy(f => JsonConvert.SerializeObject(f.AppliedAttributes)))
                        {
                            foreach (var f in group)
                            {
                                var coords = (f.Geometry as JObject)?["coordinates"] as JArray;
                                if (coords == null || coords.Count != 2) continue;

                                condensedList.Add(new ProcessedFeature
                                {
                                    GeometryType = "Point",
                                    Geometry = new JObject { ["type"] = "Point", ["coordinates"] = coords },
                                    AppliedAttributes = JsonConvert.DeserializeObject<Dictionary<string, object>>(group.Key)
                                });
                            }
                        }

                        // Combine text
                        foreach (var group in featuresList
                            .Where(f => f.GeometryType == "Text" && ((f.Geometry as JObject)?["type"]?.ToString() == "Point"))
                            .GroupBy(f => JsonConvert.SerializeObject(f.AppliedAttributes)))
                        {
                            foreach (var f in group)
                            {
                                var coords = (f.Geometry as JObject)?["coordinates"] as JArray;
                                if (coords == null || coords.Count != 2) continue;

                                var attrs = JsonConvert.DeserializeObject<Dictionary<string, object>>(group.Key);
                                string textStr = attrs.TryGetValue("text", out var tVal) && tVal is JArray tArr
                                    ? string.Join("\n", tArr.Select(t => t.ToString()))
                                    : string.Empty;

                                condensedList.Add(new ProcessedFeature
                                {
                                    GeometryType = "Text",
                                    Geometry = new JObject { ["type"] = "Point", ["coordinates"] = coords },
                                    AppliedAttributes = attrs,
                                    TextContent = textStr,
                                    FontSize = attrs.TryGetValue("size", out var ts) ? Convert.ToSingle(ts) * 12f : 14f,
                                    XOffset = attrs.TryGetValue("xOffset", out var xo) ? Convert.ToSingle(xo) : 0f,
                                    YOffset = attrs.TryGetValue("yOffset", out var yo) ? Convert.ToSingle(yo) : 0f,
                                    Underline = attrs.TryGetValue("underline", out var ul) && Convert.ToBoolean(ul)
                                });
                            }
                        }

                        features[filterIndex] = condensedList;
                    }

                    Logger.Info("GeoMap.LoadFacilityFeatures", "Created FacilityFeatures");
                }
                catch (Exception ex)
                {
                    Logger.Error("GeoMap.LoadFacilityFeatures", ex.ToString());
                }
            });

            return features;
        }

        public static string ResolveFeatureType(string geometryType, JObject props)
        {
            if (geometryType == "LineString" || geometryType == "MultiLineString")
                return "Line";
            if (geometryType == "Point" && props.ContainsKey("text"))
                return "Text";
            if (geometryType == "Point")
                return "Symbol";
            return geometryType;
        }

        public static bool IsDefaultsFeature(JObject props)
        {
            if (props == null) return false;

            return props.Value<bool?>("isLineDefaults") == true || props.Value<bool?>("isSymbolDefaults") == true || props.Value<bool?>("isTextDefaults") == true;
        }

        public static Dictionary<string, object> MergeProperties(JObject featureProps, FeatureDefaults defaults, string geometryType)
        {
            var merged = new Dictionary<string, object>();

            if (featureProps == null)
            {
                Dictionary<string, object> typeDefaultsOnly = geometryType switch
                {
                    "LineString" or "MultiLineString" => defaults.LineDefaults,
                    "Point" => defaults.SymbolDefaults,
                    _ => new Dictionary<string, object>()
                };

                foreach (var kv in typeDefaultsOnly)
                    merged[kv.Key] = kv.Value;

                ApplyCrcAutoDefaults(merged, geometryType);
                return merged;
            }

            Dictionary<string, object> typeDefaults = geometryType
                switch
            {
                "LineString" or "MultiLineString" => defaults.LineDefaults,
                "Point" => featureProps.ContainsKey("text") ? defaults.TextDefaults : defaults.SymbolDefaults,
                _ => new Dictionary<string, object>()
            };

            foreach (var kv in typeDefaults)
                merged[kv.Key] = kv.Value;

            foreach (var kv in featureProps)
                merged[kv.Key] = kv.Value;

            ApplyCrcAutoDefaults(merged, geometryType);

            return merged;
        }

        public static void ApplyCrcAutoDefaults(Dictionary<string, object> props, string featureType)
        {
            switch (featureType)
            {
                case "Line":
                    if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                    if (!props.ContainsKey("style")) props["style"] = "solid";
                    if (!props.ContainsKey("thickness")) props["thickness"] = 1;
                    break;

                case "Symbol":
                    if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                    if (!props.ContainsKey("style")) props["style"] = "vor";
                    if (!props.ContainsKey("size")) props["size"] = 1;
                    break;

                case "Text":
                    if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                    if (!props.ContainsKey("size")) props["size"] = 1;
                    if (!props.ContainsKey("underline")) props["underline"] = false;
                    if (!props.ContainsKey("xOffset")) props["xOffset"] = 0;
                    if (!props.ContainsKey("yOffset")) props["yOffset"] = 0;
                    if (!props.ContainsKey("opaque")) props["opaque"] = false;
                    break;
            }
        }

        public static FeatureDefaults ExtractLastIsDefaults(JObject geojson)
        {
            var defaults = new FeatureDefaults
            {
                LineDefaults = new Dictionary<string, object>(),
                SymbolDefaults = new Dictionary<string, object>(),
                TextDefaults = new Dictionary<string, object>()
            };

            if (geojson?["features"] is not JArray featuresArray)
                return defaults;

            foreach (var feature in featuresArray)
            {
                var props = feature["properties"] as JObject;
                if (props == null) continue;

                if (props.Value<bool?>("isLineDefaults") == true)
                    defaults.LineDefaults = props.ToObject<Dictionary<string, object>>();

                if (props.Value<bool?>("isSymbolDefaults") == true)
                    defaults.SymbolDefaults = props.ToObject<Dictionary<string, object>>();

                if (props.Value<bool?>("isTextDefaults") == true)
                    defaults.TextDefaults = props.ToObject<Dictionary<string, object>>();
            }

            return defaults;
        }
    }
}
