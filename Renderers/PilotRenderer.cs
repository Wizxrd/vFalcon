using SkiaSharp;
using System;
using System.Drawing;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Rendering
{
    public class PilotRenderer
    {
        private static SKTypeface typeface = SKTypeface.FromFile(Loader.LoadFile("Resources/Fonts", "ERAM.ttf"));
        private static string color = "#e4e400";

        private SKPaint vecPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColor.Parse(color),
            StrokeWidth = 2,
            IsAntialias = true
        };

        private SKPaint textPaint = new SKPaint
        {
            Color = SKColor.Parse(color),
            TextSize = 18f,
            IsAntialias = true,
            Typeface = typeface
        };

        public void Render(Pilot pilot, SKCanvas canvas, Size size, double scale, SKPoint panOffset)
        {
            if (pilot.GroundSpeed < 30) return;
            var screenPoint = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, pilot.Latitude, pilot.Longitude);

            float half = 3.5f;      // same as your square's half-size
            float margin = 3f;    // extension beyond the square
            float total = half + margin;

            // Callsign (add * for VFR)
            string callsign = pilot.Callsign ?? "";
            if (pilot.FlightPlan?["altitude"]?.ToString() == "VFR")
                callsign += "*";

            float textY = screenPoint.Y + half + 24;
            float textX = screenPoint.X - textPaint.MeasureText(callsign) / 2;
            canvas.DrawText(callsign, textX, textY, textPaint);

            // Altitude formatting
            string cruiseAltitudeStr = pilot.FlightPlan?["altitude"]?.ToString();
            string altitudeText = (pilot.Altitude / 100).ToString("D3");
            if (pilot.FullDataBlock)
            {
                float headingRadians = (float)(pilot.Heading * Math.PI / 180);
                float lineLength = 45f;
                float startMargin = 5f; // how far away from center to start the line
                float dx = (float)Math.Sin(headingRadians);
                float dy = (float)Math.Cos(headingRadians);
                var vecstart = new SKPoint(
                    screenPoint.X + startMargin * dx,
                    screenPoint.Y - startMargin * dy);
                var end = new SKPoint(
                    screenPoint.X + lineLength * (float)Math.Sin(headingRadians),
                    screenPoint.Y - lineLength * (float)Math.Cos(headingRadians));

                canvas.DrawLine(vecstart, end, vecPaint);

                var rect = new SKRect(screenPoint.X - half, screenPoint.Y - half, screenPoint.X + half, screenPoint.Y + half);
                canvas.DrawRect(rect, vecPaint);

                if (int.TryParse(cruiseAltitudeStr, out int cruiseAltitude) && cruiseAltitude != 0)
                {
                    string cruiseText = (cruiseAltitude / 100).ToString("D3");
                    if (Math.Abs(pilot.Altitude - cruiseAltitude) <= 300)
                    {
                        altitudeText = cruiseText + "C";
                    }
                    else if (pilot.Altitude < cruiseAltitude)
                    {
                        altitudeText = $"{cruiseText}\u2191{altitudeText}";
                    }
                    else if (pilot.Altitude > cruiseAltitude)
                    {
                        altitudeText = $"{cruiseText}\u2193{altitudeText}";
                    }
                }

                // slant
                var start = new SKPoint(rect.Left - margin, rect.Top - margin);
                var endLine = new SKPoint(rect.Right + margin, rect.Bottom + margin);
                canvas.DrawLine(start, endLine, vecPaint);

                // Speed
                string speedText = pilot.GroundSpeed.ToString();
                float altitudeY = textY + textPaint.TextSize + 2;
                canvas.DrawText(altitudeText, textX, altitudeY, textPaint);
                canvas.DrawText(speedText, textX + textPaint.MeasureText(altitudeText) + 10, altitudeY, textPaint);

                // Aircraft / arrival
                string? aircraft = pilot.FlightPlan?["aircraft_faa"]?.ToString();
                string? arrival = pilot.FlightPlan?["arrival"]?.ToString();

                if (!string.IsNullOrEmpty(aircraft))
                {
                    if (aircraft.StartsWith("H/") || aircraft.StartsWith("J/"))
                        aircraft = aircraft[2..];

                    int slashIndex = aircraft.LastIndexOf('/');
                    if (slashIndex >= 0 && slashIndex == aircraft.Length - 2)
                        aircraft = aircraft[..slashIndex];
                }

                if (!string.IsNullOrEmpty(aircraft))
                {
                    float aircraftY = altitudeY + textPaint.TextSize + 2;
                    canvas.DrawText(aircraft, textX, aircraftY, textPaint);
                    if (!string.IsNullOrEmpty(arrival))
                    {
                        float aircraftWidth = textPaint.MeasureText(aircraft);
                        canvas.DrawText(arrival, textX + aircraftWidth + 10, aircraftY, textPaint);
                    }
                }
            }
            else
            {

                // Vertical line through center
                var vStart = new SKPoint(screenPoint.X, screenPoint.Y - total);
                var vEnd = new SKPoint(screenPoint.X, screenPoint.Y + total);
                canvas.DrawLine(vStart, vEnd, vecPaint);

                // Top horizontal cap
                var topCapStart = new SKPoint(screenPoint.X - total, screenPoint.Y - total);
                var topCapEnd = new SKPoint(screenPoint.X + total, screenPoint.Y - total);
                canvas.DrawLine(topCapStart, topCapEnd, vecPaint);

                // Bottom horizontal cap
                var bottomCapStart = new SKPoint(screenPoint.X - total, screenPoint.Y + total);
                var bottomCapEnd = new SKPoint(screenPoint.X + total, screenPoint.Y + total);
                canvas.DrawLine(bottomCapStart, bottomCapEnd, vecPaint);

                float altitudeY = textY + textPaint.TextSize + 2;
                canvas.DrawText(altitudeText, textX, altitudeY, textPaint);
            }
        }
    }
}
