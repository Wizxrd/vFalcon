using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Helpers;
using vFalcon.ViewModels;

namespace vFalcon.Renderers
{
    public class RouteRenderer
    {
        private static readonly SKTypeface TypeFace = SKTypeface.FromFile(Loader.LoadFile("Resources/Fonts", "ERAM.ttf"));
        private static readonly SKColor RouteColor = SKColor.Parse("#e4e400");
        private readonly SKPaint routePaint;
        private readonly SKPaint textPaint;
        private const float squareSize = 8f;
        private const float labelOffset = 10f;

        public RouteRenderer()
        {
            routePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = RouteColor,
                StrokeWidth = 2,
                IsAntialias = true
            };

            textPaint = new SKPaint
            {
                Color = RouteColor,
                TextSize = 18f,
                Typeface = TypeFace,
                IsAntialias = true
            };

        }

        public void RenderRoute(string callsign, EramViewModel eramViewModel, SKCanvas canvas, Size size, double scale, SKPoint panOffset, JArray coords)
        {
            if (coords is null || coords.Count < 2) return;

            using var path = new SKPath();
            bool started = false;

            for (int i = 0; i < coords.Count; i++)
            {
                if (coords[i] is not JArray p || p.Count < 2) continue;

                double lat = p[0]!.Value<double>();
                double lon = p[1]!.Value<double>();

                var sp = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, lat, lon);

                if (!started) { path.MoveTo(sp); started = true; }
                else { path.LineTo(sp); }
                if (i == 0)
                {
                    var fm = textPaint.FontMetrics;
                    SKRect bounds = new();
                    textPaint.MeasureText(callsign, ref bounds);
                    float x = sp.X - bounds.MidX;
                    float y = sp.Y + labelOffset - fm.Ascent;
                    canvas.DrawText(callsign, x, y, textPaint);
                }
                if (i ==  coords.Count - 1)
                {
                    int value = eramViewModel.MapBrightness;
                    byte rgb = (byte)(value * 243 / 100);
                    SKPoint screenPoint = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, lat, lon);
                    Symbols.Airport(canvas, screenPoint, 10f, "#e4e400");
                }
                //canvas.DrawRect(sp.X - s / 2, sp.Y - s / 2, s, s, pointPaint);
            }

            if (!started) return;
            canvas.DrawPath(path, routePaint);
        }
    }
}
