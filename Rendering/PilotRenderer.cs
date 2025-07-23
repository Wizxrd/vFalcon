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

            float half = 3f;
            var rect = new SKRect(screenPoint.X - half, screenPoint.Y - half, screenPoint.X + half, screenPoint.Y + half);
            canvas.DrawRect(rect, vecPaint);

            using var textPaint = new SKPaint
            {
                Color = SKColor.Parse(color),
                TextSize = 18f,
                IsAntialias = true
               // Typeface = typeface
            };

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
                float altitudeY = textY + textPaint.TextSize + 2;
                canvas.DrawText(altitudeText, textX, altitudeY, textPaint);
            }
        }
    }
}
