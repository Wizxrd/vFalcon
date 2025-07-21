using SkiaSharp;
using System;
using System.Drawing;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Rendering
{
    public class PilotRenderer
    {
        private readonly SKTypeface typeface;

        public PilotRenderer()
        {
            typeface = SKTypeface.FromFile(Loader.LoadFile("Resources/Fonts", "ERAMv300.ttf"));
        }

        public void Render(Pilot pilot, SKCanvas canvas, Size size, double scale, SKPoint panOffset)
        {
            if (pilot.GroundSpeed < 30) return;

            string color = "#27c200";
            var screenPoint = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, pilot.Latitude, pilot.Longitude);

            // Draw vector line
            float headingRadians = (float)(pilot.Heading * Math.PI / 180);
            float lineLength = 15f;

            var end = new SKPoint(
                screenPoint.X + lineLength * (float)Math.Sin(headingRadians),
                screenPoint.Y - lineLength * (float)Math.Cos(headingRadians));

            using var vecPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColor.Parse(color),
                StrokeWidth = 2,
                IsAntialias = true
            };

            canvas.DrawLine(screenPoint, end, vecPaint);

            // Draw square
            float half = 3f;
            var rect = new SKRect(screenPoint.X - half, screenPoint.Y - half, screenPoint.X + half, screenPoint.Y + half);
            canvas.DrawRect(rect, vecPaint);

            // Draw labels (callsign, altitude, etc.)
            using var textPaint = new SKPaint
            {
                Color = SKColor.Parse(color),
                TextSize = 14f,
                IsAntialias = true,
                Typeface = typeface
            };

            float textY = screenPoint.Y + half + 24;
            canvas.DrawText(pilot.Callsign ?? "", screenPoint.X - textPaint.MeasureText(pilot.Callsign ?? "") / 2, textY, textPaint);

            // Add additional text drawing (altitude, speed, etc.) here...
        }
    }
}
