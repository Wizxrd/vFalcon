using NAudio.Gui;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Drawing;
using System.Windows.Forms;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;

namespace vFalcon.Rendering
{
    public class PilotRenderer
    {
        private EramViewModel eramViewModel;

        private static readonly SKTypeface TypeFace = SKTypeface.FromFile(Loader.LoadFile("Resources/Fonts", "ERAM.ttf"));
        private static readonly SKColor PrimaryColor = SKColor.Parse("#e4e400");
        private static readonly SKColor HistoryColor = SKColor.Parse("#525200");

        // Constants for drawing sizes
        private const float SquareHalfSize = 3.5f;
        private const float ExtensionMargin = 2f;
        private const float DataBlockLineLength = 45f;
        private const float DataBlockLineStartOffset = 5f;
        private const float DataBlockVerticalSpacing = 2f;

        private static readonly int mainPaintStroke = 2;
        private static readonly int IBeamStroke = 2;
        private static readonly int HBeamStroke = 2;
        private static readonly int historyPaintSlantStroke = 2;

        private static readonly float textSize = 12f;

        private const float IBeamSize = 6.5f;
        private const float HBeamSize = 5.5f;

        private readonly SKPaint mainPaint;
        private readonly SKPaint mainPaintIBeam;
        private readonly SKPaint mainPaintHBeam;
        private readonly SKPaint textPaint;
        private readonly SKPaint historyPaintSlant;
        private readonly SKPaint historyPaintIBeam;
        private readonly SKPaint historyPaintHBeam;

        public PilotRenderer(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            mainPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
                StrokeWidth = mainPaintStroke,
                IsAntialias = true
            };

            mainPaintIBeam = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
                StrokeWidth = IBeamStroke,
                IsAntialias = true
            };

            mainPaintHBeam = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
                StrokeWidth = HBeamStroke,
                IsAntialias = true
            };

            historyPaintSlant = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = HistoryColor,
                StrokeWidth = historyPaintSlantStroke,
                IsAntialias = true
            };

            historyPaintIBeam = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = HistoryColor,
                StrokeWidth = IBeamStroke,
                IsAntialias = true
            };

            historyPaintHBeam = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = HistoryColor,
                StrokeWidth = HBeamStroke,
                IsAntialias = true
            };

            textPaint = new SKPaint
            {
                Color = PrimaryColor,
                TextSize = textSize,
                IsAntialias = true,
                Typeface = TypeFace
            };
        }

        private static int OppositePos(int pos) => pos switch
        {
            1 => 9,
            2 => 8,
            3 => 7,
            4 => 6,
            5 => 1,
            6 => 4,
            7 => 3,
            8 => 2,
            9 => 1,
            _ => pos
        };

        public void RenderJRing(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot)
        {
            if (pilot.GroundSpeed < 30) return;

            var center = ScreenMap.CoordinateToScreen(
                size.Width, size.Height, scale, panOffset,
                pilot.Latitude, pilot.Longitude
            );

            double sKm = pilot.JRingSize * 1.852;
            double cosPhi = Math.Cos(pilot.Latitude * ScreenMap.RadPerDeg);
            if (cosPhi < 1e-6) cosPhi = 1e-6;

            float radiusPx = (float)((sKm / cosPhi) * scale);

            canvas.DrawCircle(center, radiusPx, mainPaint);

            if (pilot.FullDataBlockPosition is >= 1 and <= 9)
            {
                var posOpp = OppositePos(pilot.FullDataBlockPosition);
                var (ux, uy) = GetDir(posOpp);

                float insideOffset = 15f;
                if (posOpp == 3 || posOpp == 5 || posOpp == 6 || posOpp == 9)
                {
                    insideOffset = 15f;
                }
                else if (posOpp == 1 || posOpp == 7)
                {
                    insideOffset = 30f;
                }
                else if (posOpp == 4)
                {
                    insideOffset = 40f;
                }

                float px = center.X + ux * (radiusPx + insideOffset);
                float py = center.Y + uy * (radiusPx + insideOffset);

                var fm = textPaint.FontMetrics;
                float baselineY = py - (fm.Ascent + fm.Descent) * 0.5f;

                canvas.DrawText($"{pilot.JRingSize}NM", px, baselineY, textPaint);
            }
        }

        public void RenderPilot(Pilot pilot, SKCanvas canvas, Size size, double scale, SKPoint panOffset)
        {
            if (pilot.GroundSpeed < 30) return;

            var pilotScreenPos = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, pilot.Latitude, pilot.Longitude);

            // Altitude text
            string cruiseAltitudeRaw = pilot.FlightPlan?["altitude"]?.ToString();
            string altitudeLabel = (pilot.Altitude / 100).ToString("D3");
            string altitudeRaw = pilot.FlightPlan?["altitude"]?.ToString();
            if (string.IsNullOrEmpty(altitudeRaw) || altitudeRaw.Contains("VFR", StringComparison.OrdinalIgnoreCase))
            {
                altitudeLabel += "/VFR";
            }
            if (pilot.FullDataBlock)
            {
                DrawFullDataBlock(canvas, size, scale, panOffset, pilot, pilotScreenPos, cruiseAltitudeRaw, ref altitudeLabel);
            }
            else
            {
                DrawLimitedDataBlock(canvas, size, scale, panOffset, pilot, pilotScreenPos, altitudeLabel);
            }
        }

        private static (float ux, float uy) GetDir(int pos)
        {
            // 1 SW, 2 S, 3 SE, 4 W, 5 NE, 6 E, 7 NW, 8 N, 9 NE
            return pos switch
            {
                1 => (-0.7071f, 0.7071f),
                2 => (0.0f, 1.0f),
                3 => (0.7071f, 0.7071f),
                4 => (-1.0f, 0.0f),
                5 => (0.7071f, -0.7071f),
                6 => (1.0f, 0.0f),
                7 => (-0.7071f, -0.7071f),
                8 => (0.0f, -1.0f),
                9 => (0.7071f, -0.7071f),
                _ => (0.0f, 1.0f), // default S
            };
        }

        private void DrawFullDataBlock(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot, SKPoint pilotPos, string cruiseAltitudeRaw, ref string altitudeLabel)
        {
            var centerRect = new SKRect(pilotPos.X - SquareHalfSize, pilotPos.Y - SquareHalfSize, pilotPos.X + SquareHalfSize, pilotPos.Y + SquareHalfSize);
            canvas.DrawRect(centerRect, mainPaint);

            var (ux, uy) = GetDir(pilot.FullDataBlockPosition);

            float leaderStartGap = 2f;
            float diag = 1.41421356f;
            float edgeDist = (float)SquareHalfSize * ((Math.Abs(ux) > 0 && Math.Abs(uy) > 0) ? diag : 1f);
            float startX = pilotPos.X + ux * (edgeDist + leaderStartGap);
            float startY = pilotPos.Y + uy * (edgeDist + leaderStartGap);
            float endX = startX + ux * (pilot.LeaderLingLength * DataBlockLineLength);
            float endY = startY + uy * (pilot.LeaderLingLength * DataBlockLineLength);

            canvas.DrawLine(startX, startY, endX, endY, mainPaint);

            var fm = textPaint.FontMetrics;
            float lineGap = DataBlockVerticalSpacing;
            float lineHeight = (fm.Descent - fm.Ascent);
            float gap = 2f;

            string callsign = pilot.Callsign ?? string.Empty;

            if (int.TryParse(cruiseAltitudeRaw, out int cruiseAltitude) && cruiseAltitude != 0)
            {
                string cruiseText = (cruiseAltitude / 100).ToString("D3");
                if (Math.Abs(pilot.Altitude - cruiseAltitude) <= 300) altitudeLabel = cruiseText + "C";
                else if (pilot.Altitude < cruiseAltitude) altitudeLabel = $"{cruiseText}\u2191{altitudeLabel}";
                else altitudeLabel = $"{cruiseText}\u2193{altitudeLabel}";
            }

            string aircraftType = pilot.FlightPlan?["aircraft_faa"]?.ToString();
            string arrival = pilot.FlightPlan?["arrival"]?.ToString();
            if (!string.IsNullOrEmpty(aircraftType)) aircraftType = NormalizeAircraftType(aircraftType);
            string speedText = !string.IsNullOrEmpty(aircraftType) ? " " + pilot.GroundSpeed.ToString() : null;

            float callsignW = textPaint.MeasureText(callsign);
            float altitudeW = textPaint.MeasureText(altitudeLabel);
            float aircraftW = 0f;
            float combinedAircraftW = 0f;
            if (!string.IsNullOrEmpty(aircraftType))
            {
                aircraftW = textPaint.MeasureText(aircraftType);
                combinedAircraftW = aircraftW + (string.IsNullOrEmpty(speedText) ? 0f : textPaint.MeasureText(speedText)) + 2f;
            }
            float arrivalW = !string.IsNullOrEmpty(arrival) ? textPaint.MeasureText(arrival) : 0f;

            int numLines = 1 + 1 + (string.IsNullOrEmpty(aircraftType) ? 0 : 1) + (string.IsNullOrEmpty(arrival) ? 0 : 1);
            float blockWidth = Math.Max(Math.Max(callsignW, altitudeW), Math.Max(combinedAircraftW, arrivalW));
            float totalHeight = numLines * lineHeight + (numLines - 1) * lineGap;

            float originX, originY;
            bool east = ux > 0.5f, west = ux < -0.5f, south = uy > 0.5f, north = uy < -0.5f;
            float offsetRightExtra_2_8 = 4f; // push datablock further right for 2 & 8
            float offsetLess_1_7 = 12f; // reduce horizontal gap for 1 & 7

            if (pilot.FullDataBlockPosition == 4) // W
            {
                originX = endX - gap - blockWidth;
                originY = endY - totalHeight / 2f;
            }
            else if (pilot.FullDataBlockPosition == 6) // E
            {
                originX = endX + gap;
                originY = endY - totalHeight / 2f;
            }
            else if (pilot.FullDataBlockPosition == 2 || pilot.FullDataBlockPosition == 8) // S or N
            {
                originX = endX + gap + offsetRightExtra_2_8; // extra push to the right
                float callsignBaseline = endY - fm.Descent;
                originY = callsignBaseline + fm.Ascent;
            }
            else if (pilot.FullDataBlockPosition == 1 || pilot.FullDataBlockPosition == 7) // SW or NW
            {
                originX = west
                    ? (endX - (gap - offsetLess_1_7) - blockWidth) // slightly less left gap
                    : (endX + (gap - offsetLess_1_7));             // slightly less right gap
                float callsignBaseline = endY - fm.Descent;
                originY = callsignBaseline + fm.Ascent;
            }
            else if (pilot.FullDataBlockPosition == 3 || pilot.FullDataBlockPosition == 9) // SE or NE
            {
                originX = west ? (endX - gap - blockWidth) : (endX + gap);
                float callsignBaseline = endY - fm.Descent;
                originY = callsignBaseline + fm.Ascent;
            }
            else
            {
                originX = west ? (endX - gap - blockWidth) : (endX + gap);
                float callsignBaseline = endY - fm.Descent;
                originY = callsignBaseline + fm.Ascent;
            }

            float baseline = originY - fm.Ascent;
            float x = originX;

            canvas.DrawText(callsign, x, baseline, textPaint);
            baseline += lineHeight + lineGap;

            canvas.DrawText(altitudeLabel, x, baseline, textPaint);

            if (!string.IsNullOrEmpty(aircraftType))
            {
                baseline += lineHeight + lineGap;
                canvas.DrawText(aircraftType, x, baseline, textPaint);
                if (!string.IsNullOrEmpty(speedText)) canvas.DrawText(speedText, x + aircraftW + 2f, baseline, textPaint);
            }

            if (!string.IsNullOrEmpty(arrival))
            {
                baseline += lineHeight + lineGap;
                canvas.DrawText(arrival, x, baseline, textPaint);
            }

            int mins = eramViewModel.VelocityVector;
            double distanceNm = pilot.GroundSpeed * (mins / 60.0);
            var (predLat, predLon) = DestinationPointNM(pilot.Latitude, pilot.Longitude, pilot.Heading, distanceNm);
            var predicted = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, predLat, predLon);
            SKPoint v = new SKPoint(predicted.X - pilotPos.X, predicted.Y - pilotPos.Y);
            float vLen = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
            SKPoint vvStart = vLen > 1e-3 ? new SKPoint(pilotPos.X + v.X / vLen * DataBlockLineStartOffset, pilotPos.Y + v.Y / vLen * DataBlockLineStartOffset) : pilotPos;
            canvas.DrawLine(vvStart, predicted, mainPaint);

            var slantStart = new SKPoint(centerRect.Left - ExtensionMargin, centerRect.Top - ExtensionMargin);
            var slantEnd = new SKPoint(centerRect.Right + ExtensionMargin, centerRect.Bottom + ExtensionMargin);
            canvas.DrawLine(slantStart, slantEnd, mainPaint);
        }

        private void DrawLimitedDataBlock(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot, SKPoint pilotPos, string altitudeLabel)
        {

            float labelYStart = pilotPos.Y + IBeamSize;
            string callsign = pilot.Callsign ?? string.Empty;
            float callsignX = pilotPos.X + 10f;
            canvas.DrawText(callsign, callsignX, labelYStart, textPaint);

            canvas.DrawLine(new SKPoint(pilotPos.X - HBeamSize, pilotPos.Y - IBeamSize), new SKPoint(pilotPos.X + HBeamSize, pilotPos.Y - IBeamSize), mainPaintHBeam);
            canvas.DrawLine(new SKPoint(pilotPos.X - HBeamSize, pilotPos.Y + IBeamSize), new SKPoint(pilotPos.X + HBeamSize, pilotPos.Y + IBeamSize), mainPaintHBeam);
            canvas.DrawLine(new SKPoint(pilotPos.X, pilotPos.Y - IBeamSize), new SKPoint(pilotPos.X, pilotPos.Y + IBeamSize), mainPaintIBeam);

            float altitudeY = labelYStart + textPaint.TextSize + DataBlockVerticalSpacing;
            canvas.DrawText(altitudeLabel, callsignX, altitudeY, textPaint);
        }

        public void RenderHistory(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot)
        {
            if (pilot.GroundSpeed < 30) return;
            if (pilot.FullDataBlock)
            {
                if (pilot.History == null || pilot.History.Count < 1)
                    return;

                for (int i = 1; i < pilot.History.Count - 1; i++)
                {
                    double lat = pilot.History[i].Lat;
                    double lon = pilot.History[i].Lon;
                    SKPoint historyPosition = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, lat, lon);
                    var centerRect = new SKRect(historyPosition.X - SquareHalfSize, historyPosition.Y - SquareHalfSize, historyPosition.X + SquareHalfSize, historyPosition.Y + SquareHalfSize);
                    var slantStart = new SKPoint(centerRect.Left - ExtensionMargin, centerRect.Top - ExtensionMargin);
                    var slantEnd = new SKPoint(centerRect.Right + ExtensionMargin, centerRect.Bottom + ExtensionMargin);
                    canvas.DrawLine(slantStart, slantEnd, historyPaintSlant);
                }
            }
            else
            {

                if (pilot.History == null || pilot.History.Count < 1)
                    return;
                for (int i = 1; i < pilot.History.Count - 1; i++)
                {
                    double lat = pilot.History[i].Lat;
                    double lon = pilot.History[i].Lon;
                    SKPoint historyPosition = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, lat, lon);
                    canvas.DrawLine(new SKPoint(historyPosition.X - HBeamSize, historyPosition.Y - IBeamSize), new SKPoint(historyPosition.X + HBeamSize, historyPosition.Y - IBeamSize), historyPaintHBeam);
                    canvas.DrawLine(new SKPoint(historyPosition.X - HBeamSize, historyPosition.Y + IBeamSize), new SKPoint(historyPosition.X + HBeamSize, historyPosition.Y + IBeamSize), historyPaintHBeam);
                    canvas.DrawLine(new SKPoint(historyPosition.X, historyPosition.Y - IBeamSize), new SKPoint(historyPosition.X, historyPosition.Y + IBeamSize), historyPaintIBeam);
                }
            }
        }

        private static string NormalizeAircraftType(string rawType)
        {
            if (rawType.StartsWith("H/") || rawType.StartsWith("J/"))
                rawType = rawType[2..];

            int slashIndex = rawType.LastIndexOf('/');
            if (slashIndex >= 0 && slashIndex == rawType.Length - 2)
                rawType = rawType[..slashIndex];

            return rawType;
        }

        private static (double lat, double lon) DestinationPointNM(double latDeg, double lonDeg, double bearingDeg, double distanceNm)
        {
            double φ1 = latDeg * ScreenMap.RadPerDeg;
            double λ1 = lonDeg * ScreenMap.RadPerDeg;
            double θ = bearingDeg * ScreenMap.RadPerDeg;
            double δ = distanceNm / ScreenMap.EarthRadiusNM;

            double sinφ2 = Math.Sin(φ1) * Math.Cos(δ) + Math.Cos(φ1) * Math.Sin(δ) * Math.Cos(θ);
            double φ2 = Math.Asin(sinφ2);
            double y = Math.Sin(θ) * Math.Sin(δ) * Math.Cos(φ1);
            double x = Math.Cos(δ) - Math.Sin(φ1) * sinφ2;
            double λ2 = λ1 + Math.Atan2(y, x);

            double lat2 = φ2 / ScreenMap.RadPerDeg;
            double lon2 = (λ2 / ScreenMap.RadPerDeg + 540) % 360 - 180;
            return (lat2, lon2);
        }
    }
}
