using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Renderers
{
    public class VideoMap
    {

        public static void CenterAtCoordinates(int width, int height, double scale, ref SKPoint panOffset, double lat, double lon)
        {
            var screenPoint = ScreenMap.CoordinateToScreen(width, height, scale, panOffset, lat, lon);
            float shiftX = (width / 2f) - screenPoint.X;
            float shiftY = (height / 2f) - screenPoint.Y;
            panOffset.X += shiftX;
            panOffset.Y += shiftY;
        }

        public void Render(SKCanvas canvas, System.Drawing.Size clientSize, double scale, SKPoint panOffset, IEnumerable<ProcessedFeature> features)
        {
            try
            {
                foreach (var feature in features)
                {
                    if (feature.GeometryType != "Line") continue;
                    // Extract coordinates (assuming feature.Geometry is JToken or something similar)
                    var coordsArray = ((JObject)feature.Geometry)["coordinates"] as JArray;
                    if (coordsArray.Count < 2)
                        continue;

                    // Determine feature color (use BCG or fallback)
                    SKColor color = SKColors.White;
                    //if (feature.AppliedAttributes.TryGetValue("bcg", out var bcgValue)) color = MapColorFromBCG(Convert.ToInt32(bcgValue));
                    Logger.Debug("COLOR", color.ToString());
                    // Create paint using feature style
                    using var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        Color = color,
                        StrokeWidth = feature.AppliedAttributes.TryGetValue("thickness", out var thickness) ? Convert.ToSingle(thickness) : 1f,
                        IsAntialias = true,
                        PathEffect = ResolvePathEffect(feature.AppliedAttributes)
                    };

                    // Build the path
                    using var path = new SKPath();
                    bool isFirst = true;
                    foreach (var coord in coordsArray)
                    {
                        double lon = coord[0].Value<double>();
                        double lat = coord[1].Value<double>();
                        var point = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale, panOffset, lat, lon);

                        if (isFirst)
                        {
                            path.MoveTo(point);
                            isFirst = false;
                        }
                        else
                        {
                            path.LineTo(point);
                        }
                    }

                    canvas.DrawPath(path, paint);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("VideoMap.Render", ex.ToString());
            }
        }

        private SKColor MapColorFromBCG(int bcg)
        {
            // Map BCG index to colors (customize as needed)
            return bcg switch
            {
                1 => SKColors.LightGreen,
                2 => SKColors.Cyan,
                3 => SKColors.Orange,
                _ => SKColors.Gray
            };
        }

        private SKPathEffect ResolvePathEffect(Dictionary<string, object> attributes)
        {
            if (attributes.TryGetValue("style", out var style))
            {
                string styleStr = style.ToString();
                if (styleStr == "ShortDashed")
                    return SKPathEffect.CreateDash(new float[] { 10, 5 }, 0);
                else if (styleStr == "LongDashed")
                {
                    return SKPathEffect.CreateDash(new float[] {15, 5}, 0);
                }
                else if (styleStr == "LongDashShortDash")
                {
                    return SKPathEffect.CreateDash(new float[] { 10, 5, 5, 5 }, 0);
                }
            }
            return null;
        }
    }
}
