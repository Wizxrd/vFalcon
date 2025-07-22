// Rendering/VideoMap.cs
using GeoJSON.Net.Geometry;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Rendering
{
    public class VideoMap
    {
        private readonly Dictionary<string, List<List<Coordinate>>> loadedMaps = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SKColor> fileColors = new();
        public List<string> ActiveMaps { get; } = new();

        private readonly SKPaint dashPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true,
            PathEffect = SKPathEffect.CreateDash(new float[] { 10, 20 }, 0)
        };

        public static void CenterAtCoordinates(int width, int height, double scale, ref SKPoint panOffset, double lat, double lon)
        {
            var screenPoint = ScreenMap.CoordinateToScreen(width, height, scale, panOffset, lat, lon);
            float shiftX = (width / 2f) - screenPoint.X;
            float shiftY = (height / 2f) - screenPoint.Y;
            panOffset.X += shiftX;
            panOffset.Y += shiftY;
        }

        public void Load(string file, string color)
        {
            try
            {
                string jsonText = File.ReadAllText(file);
                var featureCollection = JObject.Parse(jsonText);
                var fileLines = new List<List<Coordinate>>();

                foreach (var feature in featureCollection["features"]!)
                {
                    var geometryType = feature["geometry"]?["type"]?.ToString();
                    var coordinatesToken = feature["geometry"]?["coordinates"];

                    if (geometryType == "LineString")
                    {
                        var coordinates = coordinatesToken!
                            .ToObject<List<List<double>>>()!
                            .Select(coord => new Coordinate(coord[1], coord[0]))
                            .ToList();

                        fileLines.Add(coordinates);
                    }
                    else if (geometryType == "Polygon")
                    {
                        foreach (var ringToken in coordinatesToken!)
                        {
                            var coordinates = ringToken!
                                .ToObject<List<List<double>>>()!
                                .Select(coord => new Coordinate(coord[1], coord[0]))
                                .ToList();

                            fileLines.Add(coordinates);
                        }
                    }
                }

                string fileName = Path.GetFileName(file);
                loadedMaps[fileName] = fileLines;
                fileColors[fileName] = SKColor.Parse(color);
                ActiveMaps.Add(fileName);
                Logger.Debug("VideoMap.Load", $"Loaded: \"{fileName}\"");
            }
            catch (Exception ex)
            {
                Logger.Error("VideoMap.Load", ex.ToString());
            }
        }

        public void Unload(string file)
        {
            string fileName = Path.GetFileName(file);
            if (loadedMaps.Remove(fileName))
            {
                ActiveMaps.Remove(fileName);
                Logger.Debug("VideoMap.Load", $"Unloaded: \"{fileName}\"");
            }
        }

        public void Render(SKCanvas canvas, Size clientSize, double scale, SKPoint panOffset)
        {
            try
            {
                foreach (var (fileName, fileLines) in loadedMaps)
                {
                    dashPaint.Color = fileColors.TryGetValue(fileName, out var val) ? val : SKColors.Gray;

                    foreach (var line in fileLines)
                    {
                        if (line.Count < 2) continue;

                        using var path = new SKPath();
                        var first = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale, panOffset, line[0].Latitude, line[0].Longitude);
                        path.MoveTo(first);

                        for (int i = 1; i < line.Count; i++)
                        {
                            var pt = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale, panOffset, line[i].Latitude, line[i].Longitude);
                            path.LineTo(pt);
                        }

                        canvas.DrawPath(path, dashPaint);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("VideoMap.Render", ex.ToString());
            }
        }
    }
}