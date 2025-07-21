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
            }
        }

        public void Render(SKCanvas canvas, Size clientSize, double scale, SKPoint panOffset)
        {
            try
            {
                foreach (var (fileName, fileLines) in loadedMaps)
                {
                    var color = fileColors.TryGetValue(fileName, out var val) ? val : SKColors.Gray;

                    using var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        Color = color,
                        StrokeWidth = 1,
                        IsAntialias = true
                        //PathEffect = SKPathEffect.CreateDash(new float[] { 10, 20 }, 0)
                    };

                    foreach (var lineString in fileLines)
                    {
                        if (lineString.Count < 2) continue;

                        using var path = new SKPath();
                        path.MoveTo(ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale, panOffset, lineString[0].Latitude, lineString[0].Longitude));

                        foreach (var coord in lineString.Skip(1))
                        {
                            var pt = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale, panOffset, coord.Latitude, coord.Longitude);
                            path.LineTo(pt);
                        }
                        canvas.DrawPath(path, paint);
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