using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;

namespace vFalcon.Renderers
{
    public class VideoMap
    {

        private static readonly SKTypeface EramTypeface = SKTypeface.FromFile(Loader.LoadFile("Resources/Fonts", "ERAM.ttf"));
        private static readonly SKColor DefaultColor = SKColor.Parse("#757575");

        private readonly SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            Color = DefaultColor,
            IsAntialias = true,
        };

        private readonly SKPaint textPaint = new SKPaint
        {
            Typeface = EramTypeface,
            TextSize = 12f,
            Color = DefaultColor,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        public static void CenterAtCoordinates(int width, int height, double scale, ref SKPoint panOffset, double lat, double lon)
        {
            var screenPoint = ScreenMap.CoordinateToScreen(width, height, scale, panOffset, lat, lon);
            float shiftX = (width / 2f) - screenPoint.X;
            float shiftY = (height / 2f) - screenPoint.Y;
            panOffset.X += shiftX;
            panOffset.Y += shiftY;
        }

        public void Render(EramViewModel eramViewModel, SKCanvas canvas, System.Drawing.Size clientSize, double scale, SKPoint panOffset, IEnumerable<ProcessedFeature> features)
        {
            try
            {
                foreach (var feature in features)
                {
                    try
                    {
                        string geomType = feature.GeometryType switch
                        {
                            "Line" => "LineString",
                            _ => feature.GeometryType
                        };

                        var geomObj = feature.Geometry as JObject ?? (feature.Geometry is JToken jt ? (JObject)jt : null);
                        if (geomObj == null) continue;

                        var coordsToken = geomObj["coordinates"];
                        if (coordsToken == null) continue;

                        string style = (feature.AppliedAttributes.TryGetValue("style", out var styleValue) && styleValue != null) ? styleValue.ToString() : "OtherWaypoints";

                        //paint.StrokeWidth = feature.AppliedAttributes.TryGetValue("thickness", out var thickness) ? Convert.ToSingle(thickness) : 1f;
                        paint.PathEffect = ResolvePathEffect(feature.AppliedAttributes);
                        int value = eramViewModel.MapBrightness;
                        byte rgb = (byte)(value * 243 / 100);
                        if (geomType == "LineString")
                        {
                            if (coordsToken is JArray arr)
                                DrawLineString(canvas, arr, paint, clientSize, scale, panOffset, rgb);
                        }
                        else if (geomType == "MultiLineString")
                        {
                            if (coordsToken is JArray multiArray)
                                DrawMultiLineSegments(canvas, multiArray, paint, clientSize, scale, panOffset, rgb);
                        }
                        else if (geomType == "Point")
                        {
                            double lon = coordsToken[0].Value<double>();
                            double lat = coordsToken[1].Value<double>();
                            SKPoint screenPoint = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale, panOffset, lat, lon);
                            DrawSymbol(canvas, style, screenPoint, rgb);
                        }
                        else if (geomType == "Text")
                        {
                            double lon = coordsToken[0].Value<double>();
                            double lat = coordsToken[1].Value<double>();
                            SKPoint screenPoint = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale, panOffset, lat, lon);
                            DrawTextLabel(canvas, feature.TextContent, screenPoint, feature.FontSize, feature.XOffset, feature.YOffset*1.5f, feature.Underline, rgb);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("VideoMap.Render", ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("VideoMap.Render", ex.ToString());
            }
        }

        private void DrawSymbol(SKCanvas canvas, string style, SKPoint center, byte rgb)
        {
            switch (style.ToLower())
            {
                case "obstruction1": Symbols.Obstruction1(canvas, center, rgb); break;
                case "obstruction2": Symbols.Obstruction2(canvas, center, rgb); break;
                case "helipad": Symbols.Helipad(canvas, center, rgb); break;
                case "nuclear": Symbols.Nuclear(canvas, center, rgb); break;
                case "emergencyairport": Symbols.EmergencyAirport(canvas, center, rgb); break;
                case "radar": Symbols.Radar(canvas, center, rgb); break;
                case "iaf": Symbols.Iaf(canvas, center, rgb); break;
                case "rnavonlywaypoint": Symbols.RnavOnlyWaypoint(canvas, center, rgb); break;
                case "rnav": Symbols.Rnav(canvas, center, rgb); break;
                case "airwayintersection": Symbols.AirwayIntersection(canvas, center, rgb); break;
                case "ndb": Symbols.Ndb(canvas, center, rgb); break;
                case "vor": Symbols.Vor(canvas, center, rgb); break;
                case "otherwaypoints": Symbols.OtherWaypoints(canvas, center, rgb); break;
                case "airport": Symbols.Airport(canvas, center, rgb); break;
                case "satelliteairport": Symbols.SatelliteAirport(canvas, center, rgb); break;
                case "tacan": Symbols.Tacan(canvas, center, rgb); break;
                case "dme": Symbols.Tacan(canvas, center, rgb); break;
            }
        }

        private void DrawTextLabel(SKCanvas canvas, string text, SKPoint position, float fontSize, float xOffset, float yOffset, bool underline, byte rgb)
        {
            SKColor color = new SKColor(rgb, rgb, rgb);
            textPaint.Color = color;
            if (string.IsNullOrEmpty(text)) return;

            SKPoint drawPoint = new SKPoint(position.X + xOffset, position.Y - yOffset);
            string[] lines = text.Split('\n');
            float lineHeight = fontSize * 1.2f;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                SKPoint linePos = new SKPoint(drawPoint.X, drawPoint.Y + (i * lineHeight));
                canvas.DrawText(line, linePos, textPaint);

                if (underline)
                {
                    float textWidth = textPaint.MeasureText(line);
                    float underlineY = linePos.Y + 2;
                    canvas.DrawLine(linePos.X, underlineY, linePos.X + textWidth, underlineY, textPaint);
                }
            }
        }

        private void DrawMultiLineSegments(SKCanvas canvas, JArray array, SKPaint paint, System.Drawing.Size clientSize, double scale, SKPoint panOffset, byte rgb)
        {
            foreach (var item in array)
            {
                if (item is JArray subArray)
                {
                    if (subArray.First is JArray firstInner && firstInner.First is JArray)
                        DrawMultiLineSegments(canvas, subArray, paint, clientSize, scale, panOffset, rgb);
                    else
                        DrawLineString(canvas, subArray, paint, clientSize, scale, panOffset, rgb);
                }
            }
        }

        private void DrawLineString(SKCanvas canvas, JArray coordsArray, SKPaint paint, System.Drawing.Size clientSize, double scale, SKPoint panOffset, byte rgb)
        {
            if (coordsArray == null || coordsArray.Count < 2) return;

            using var path = new SKPath();
            bool isFirst = true;
            SKColor color = new SKColor(rgb, rgb, rgb);
            paint.Color = color;
            foreach (var pt in coordsArray)
            {
                if (pt is not JArray pointArray || pointArray.Count < 2) continue;
                if (pointArray[0].Type != JTokenType.Float && pointArray[0].Type != JTokenType.Integer) continue;
                if (pointArray[1].Type != JTokenType.Float && pointArray[1].Type != JTokenType.Integer) continue;

                double lon = pointArray[0].ToObject<double>();
                double lat = pointArray[1].ToObject<double>();

                SKPoint screenPoint = ScreenMap.CoordinateToScreen(clientSize.Width, clientSize.Height, scale, panOffset, lat, lon);

                if (isFirst)
                {
                    path.MoveTo(screenPoint);
                    isFirst = false;
                }
                else
                {
                    path.LineTo(screenPoint);
                }
            }

            if (!isFirst)
                canvas.DrawPath(path, paint);
        }

        private SKPathEffect ResolvePathEffect(Dictionary<string, object> attributes)
        {
            if (attributes.TryGetValue("style", out var style) && style != null)
            {
                string styleStr = style.ToString().ToLowerInvariant();

                if (styleStr == "shortdashed")
                {
                    return SKPathEffect.CreateDash(new float[] { 10, 20 }, 0);
                }
                else if (styleStr == "longdashed")
                {
                    return SKPathEffect.CreateDash(new float[] { 20, 30 }, 0);
                }
                else if (styleStr == "longdashshortdash")
                {
                    return SKPathEffect.CreateDash(new float[] { 10, 20, 10, 20 }, 0);
                }
            }
            return null;
        }
    }
}
