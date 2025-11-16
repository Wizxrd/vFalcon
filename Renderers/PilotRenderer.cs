using NAudio.Gui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Ink;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;

namespace vFalcon.Rendering
{
    public class PilotRenderer
    {
        private EramViewModel eramViewModel;

        private static readonly SKTypeface TypeFace = SKTypeface.FromFile(Loader.LoadFile("Resources/Fonts", "ERAM.ttf"));
        public SKColor PrimaryColor = SKColor.Parse("#e4e400");
        public SKColor StarsFullDbColor = SKColors.White;
        public SKColor StarsLimDbColor = SKColors.LimeGreen;
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
        private static readonly int dwellLockStroke = 1;

        public float textSize = 12f;

        private const float IBeamSize = 6.5f;
        private const float HBeamSize = 5.5f;

        private readonly SKPaint fullPaint;
        public SKPaint mainPaint;
        public SKPaint limitedPaint;
        private readonly SKPaint mainPaintIBeam;
        private readonly SKPaint mainPaintHBeam;
        public SKPaint textPaint;
        public SKPaint fullDatablockTextPaint;
        public SKPaint limitedDatablockTextPaint;
        public SKPaint historyPaintSlant;
        public SKPaint historyPaintIBeam;
        public SKPaint historyPaintHBeam;
        private readonly SKPaint dwellLockPaint;

        public SKPaint starsLimitedDatablockPaint;
        public SKPaint starsDatablockPaint;
        public SKPaint starsHistoryPaint;
        public SKPaint starsLimitedPaint;
        public SKPaint starsFullPaint;

        public PilotRenderer(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;

            starsFullPaint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                TextSize = 12f
            };

            starsLimitedPaint = new SKPaint
            {
                Color = SKColors.Lime,
                StrokeWidth = 1,
                IsAntialias = true,
                TextSize = 12f
            };

            starsHistoryPaint = new SKPaint {
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            starsLimitedDatablockPaint = new SKPaint
            {
                Color = SKColors.LimeGreen,
                StrokeWidth = 1,
                IsAntialias = true,
                TextSize = textSize
            };

            starsDatablockPaint = new SKPaint {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                TextSize = textSize
            };
            fullPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
                StrokeWidth = mainPaintStroke,
                IsAntialias = true
            };
            mainPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
                StrokeWidth = mainPaintStroke,
                IsAntialias = true
            };

            limitedPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
                TextSize = textSize,
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
                Color = PrimaryColor,
                StrokeWidth = historyPaintSlantStroke,
                IsAntialias = true
            };

            historyPaintIBeam = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
                StrokeWidth = IBeamStroke,
                IsAntialias = true
            };

            historyPaintHBeam = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
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

            fullDatablockTextPaint = new SKPaint
            {
                Color = PrimaryColor,
                TextSize = textSize,
                IsAntialias = true,
                Typeface = TypeFace
            };

            limitedDatablockTextPaint = new SKPaint
            {
                Color = PrimaryColor,
                TextSize = textSize,
                IsAntialias = true,
                Typeface = TypeFace
            };

            dwellLockPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = PrimaryColor,
                StrokeWidth = dwellLockStroke,
                IsAntialias = true
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

            textPaint.Color = SKColor.Parse("#e4e400");
            mainPaint.Color = SKColor.Parse("#e4e400");

            if (pilot.DatablockType == "STARS" & textPaint.Color != SKColor.Parse("#1e78ff"))
            {
                textPaint.Color = SKColor.Parse("#1e78ff");
                mainPaint.Color = SKColor.Parse("#1e78ff");
            }

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
                if (pilot.DatablockType == "ERAM")
                    DrawEramFullDataBlock(canvas, size, scale, panOffset, pilot, pilotScreenPos, cruiseAltitudeRaw, ref altitudeLabel);
                else if (pilot.DatablockType == "STARS")
                {
                    DrawStarsFullDatablock(canvas, size, scale, panOffset, pilot, pilotScreenPos, cruiseAltitudeRaw, ref altitudeLabel);
                }
            }
            else
            {
                if (pilot.DatablockType == "ERAM")
                    DrawEramLimitedDataBlock(canvas, size, scale, panOffset, pilot, pilotScreenPos, altitudeLabel);
                else if (pilot.DatablockType == "STARS")
                {
                    DrawStarsLimitedDatablock(canvas, size, scale, panOffset, pilot, pilotScreenPos, altitudeRaw, altitudeLabel);
                }
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

        private void DrawEramFullDataBlock(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot, SKPoint pilotPos, string cruiseAltitudeRaw, ref string altitudeLabel)
        {
            var centerRect = new SKRect(pilotPos.X - SquareHalfSize, pilotPos.Y - SquareHalfSize, pilotPos.X + SquareHalfSize, pilotPos.Y + SquareHalfSize);
            canvas.DrawRect(centerRect, fullPaint);

            var (ux, uy) = GetDir(pilot.FullDataBlockPosition);

            float leaderStartGap = 2f;
            float diag = 1.4f;
            float edgeDist = (float)SquareHalfSize * ((Math.Abs(ux) > 0 && Math.Abs(uy) > 0) ? diag : 1f);
            float startX = pilotPos.X + ux * (edgeDist + leaderStartGap);
            float startY = pilotPos.Y + uy * (edgeDist + leaderStartGap);
            float endX = startX + ux * (pilot.LeaderLingLength * DataBlockLineLength);
            float endY = startY + uy * (pilot.LeaderLingLength * DataBlockLineLength);
            canvas.DrawLine(startX, startY, endX, endY, mainPaint);

            var p = fullDatablockTextPaint;
            var fm = p.FontMetrics;
            float lineGap = DataBlockVerticalSpacing;
            float lineAdvance = (fm.Descent - fm.Ascent) + fm.Leading;
            float gap = 2f;

            string callsign = pilot.Callsign ?? string.Empty;

            if (int.TryParse(cruiseAltitudeRaw, out int cruiseAltitude) && cruiseAltitude != 0)
            {
                string cruiseText = (cruiseAltitude / 100).ToString("D3");
                if (Math.Abs(pilot.Altitude - cruiseAltitude) <= 500) altitudeLabel = cruiseText + "C";
                else if (pilot.Altitude < cruiseAltitude) altitudeLabel = $"{cruiseText}\u2191{altitudeLabel}";
                else altitudeLabel = $"{cruiseText}\u2193{altitudeLabel}";
            }

            string aircraftType = pilot.FlightPlan?["aircraft_faa"]?.ToString();
            string arrival = pilot.FlightPlan?["arrival"]?.ToString();
            if (!string.IsNullOrEmpty(aircraftType)) aircraftType = NormalizeAircraftType(aircraftType);
            string speedText = !string.IsNullOrEmpty(aircraftType) ? " " + pilot.GroundSpeed.ToString() : null;

            float callsignW = p.MeasureText(callsign);
            float altitudeW = p.MeasureText(altitudeLabel);
            float aircraftW = 0f;
            float combinedAircraftW = 0f;
            if (!string.IsNullOrEmpty(aircraftType))
            {
                aircraftW = p.MeasureText(aircraftType);
                combinedAircraftW = aircraftW + (string.IsNullOrEmpty(speedText) ? 0f : p.MeasureText(speedText)) + 2f;
            }
            float arrivalW = !string.IsNullOrEmpty(arrival) ? p.MeasureText(arrival) : 0f;

            int numLines = 1 + 1 + (string.IsNullOrEmpty(aircraftType) ? 0 : 1) + (string.IsNullOrEmpty(arrival) ? 0 : 1);
            float blockWidth = Math.Max(Math.Max(callsignW, altitudeW), Math.Max(combinedAircraftW, arrivalW));
            float totalHeight = numLines * lineAdvance + (numLines - 1) * lineGap;

            float originX, originY;
            bool east = ux > 0.5f, west = ux < -0.5f, south = uy > 0.5f, north = uy < -0.5f;
            float offsetRightExtra_2_8 = 4f;
            float offsetLess_1_7 = 12f;

            const float BoxPadX = 4f;
            const float BoxPadY = 3f;
            const float LineClear = 4f;
            const float DiagonalExtra17 = 10f;
            float baseGap = gap + LineClear + BoxPadX;

            if (pilot.FullDataBlockPosition == 4)
            {
                originX = endX - (gap + LineClear + BoxPadX) - blockWidth;
                originY = endY - totalHeight / 2f;
            }
            else if (pilot.FullDataBlockPosition == 6)
            {
                originX = endX + (gap + LineClear + BoxPadX);
                originY = endY - totalHeight / 2f;
            }
            else if (pilot.FullDataBlockPosition == 2 || pilot.FullDataBlockPosition == 8)
            {
                originX = endX + gap + offsetRightExtra_2_8 + (LineClear + BoxPadX);
                float callsignBaseline = endY - fm.Descent;
                originY = callsignBaseline + fm.Ascent;
            }
            else if (pilot.FullDataBlockPosition == 1 || pilot.FullDataBlockPosition == 7)
            {
                float sideGap = (gap - offsetLess_1_7) + baseGap + DiagonalExtra17;
                originX = west ? endX - sideGap - blockWidth : endX + sideGap;
                float callsignBaseline = endY - fm.Descent;
                originY = callsignBaseline + fm.Ascent;
            }
            else if (pilot.FullDataBlockPosition == 3 || pilot.FullDataBlockPosition == 9)
            {
                originX = west ? endX - (gap + LineClear + BoxPadX) - blockWidth : endX + (gap + LineClear + BoxPadX);
                float callsignBaseline = endY - fm.Descent;
                originY = callsignBaseline + fm.Ascent;
            }
            else
            {
                originX = west ? endX - (gap + LineClear + BoxPadX) - blockWidth : endX + (gap + LineClear + BoxPadX);
                float callsignBaseline = endY - fm.Descent;
                originY = callsignBaseline + fm.Ascent;
            }

            float baseline = originY - fm.Ascent;
            float x = originX;

            canvas.DrawText(callsign, x, baseline, p);
            baseline += lineAdvance + lineGap;

            canvas.DrawText(altitudeLabel, x, baseline, p);

            if (!string.IsNullOrEmpty(aircraftType))
            {
                baseline += lineAdvance + lineGap;
                canvas.DrawText(aircraftType, x, baseline, p);
                if (!string.IsNullOrEmpty(speedText)) canvas.DrawText(speedText, x + aircraftW + 2f, baseline, p);
            }

            if (!string.IsNullOrEmpty(arrival))
            {
                baseline += lineAdvance + lineGap;
                canvas.DrawText(arrival, x, baseline, p);
            }

            int mins = eramViewModel.VelocityVector;
            double distanceNm = pilot.GroundSpeed * (mins / 60.0);
            var (predLat, predLon) = DestinationPointNM(pilot.Latitude, pilot.Longitude, pilot.Heading, distanceNm);
            var predicted = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, predLat, predLon);
            SKPoint v = new SKPoint(predicted.X - pilotPos.X, predicted.Y - pilotPos.Y);
            float vLen = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
            SKPoint vvStart = vLen > 1e-3 ? new SKPoint(pilotPos.X + v.X / vLen * DataBlockLineStartOffset, pilotPos.Y + v.Y / vLen * DataBlockLineStartOffset) : pilotPos;
            canvas.DrawLine(vvStart, predicted, fullPaint);

            var slantStart = new SKPoint(centerRect.Left - ExtensionMargin, centerRect.Top - ExtensionMargin);
            var slantEnd = new SKPoint(centerRect.Right + ExtensionMargin, centerRect.Bottom + ExtensionMargin);
            canvas.DrawLine(slantStart, slantEnd, fullPaint);

            if (pilot.DwellLock)
            {
                var box = SKRect.Create(
                    originX - BoxPadX,
                    originY - BoxPadY,
                    blockWidth + BoxPadX * 2f,
                    totalHeight + BoxPadY * 2f);
                canvas.DrawRect(box, dwellLockPaint);
            }
        }

        private void DrawEramLimitedDataBlock(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot, SKPoint pilotPos, string altitudeLabel)
        {
            float callsignBaseline = pilotPos.Y + IBeamSize;
            string callsign = pilot.Callsign ?? string.Empty;
            float callsignX = pilotPos.X + 15f;

            var fm = limitedDatablockTextPaint.FontMetrics;
            float lineHeight = (fm.Descent - fm.Ascent) + fm.Leading;
            float lineGap = DataBlockVerticalSpacing;

            float callsignW = limitedDatablockTextPaint.MeasureText(callsign);
            canvas.DrawText(callsign, callsignX, callsignBaseline, limitedDatablockTextPaint);

            canvas.DrawLine(new SKPoint(pilotPos.X - HBeamSize, pilotPos.Y - IBeamSize), new SKPoint(pilotPos.X + HBeamSize, pilotPos.Y - IBeamSize), mainPaintHBeam);
            canvas.DrawLine(new SKPoint(pilotPos.X - HBeamSize, pilotPos.Y + IBeamSize), new SKPoint(pilotPos.X + HBeamSize, pilotPos.Y + IBeamSize), mainPaintHBeam);
            canvas.DrawLine(new SKPoint(pilotPos.X, pilotPos.Y - IBeamSize), new SKPoint(pilotPos.X, pilotPos.Y + IBeamSize), mainPaintIBeam);

            float altitudeBaseline = callsignBaseline + lineHeight + lineGap;
            float altitudeW = limitedDatablockTextPaint.MeasureText(altitudeLabel);
            canvas.DrawText(altitudeLabel, callsignX, altitudeBaseline, limitedDatablockTextPaint);

            if (pilot.DwellLock)
            {
                float top = Math.Min(callsignBaseline + fm.Ascent, altitudeBaseline + fm.Ascent);
                float bottom = Math.Max(callsignBaseline + fm.Descent, altitudeBaseline + fm.Descent);
                float height = bottom - top;

                float padX = 4f, padY = 4f;
                float width = Math.Max(callsignW, altitudeW);
                var box = SKRect.Create(callsignX - padX, top - padY, width + padX * 2f, height + padY * 2f);
                canvas.DrawRect(box, dwellLockPaint);
            }
        }

        private void DrawStarsFullDatablock(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot, SKPoint pilotPos, string cruiseAltitudeRaw, ref string altitudeLabel)
        {
            var circlePaint = new SKPaint { Color = SKColor.Parse("#1e78ff"), IsAntialias = true, Style = SKPaintStyle.Fill };
            var positionPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };

            string mark = string.IsNullOrWhiteSpace(pilot.StarsSectorId) ? "*" : pilot.StarsSectorId;
            using var font = new SKFont(TypeFace, starsDatablockPaint.TextSize) { Subpixel = true };
            font.MeasureText(mark, out SKRect bounds);
            float x = pilotPos.X - (bounds.Left + bounds.Width * 0.5f);
            float y = pilotPos.Y - (bounds.Top + bounds.Height * 0.5f);
            using var outlinePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                StrokeJoin = SKStrokeJoin.Round
            };
            var (ux, uy) = GetDir(pilot.FullDataBlockPosition);
            float leaderStartGap = 2f;
            float diag = 1.4f;
            float edgeDist = (float)SquareHalfSize * ((Math.Abs(ux) > 0 && Math.Abs(uy) > 0) ? diag : 1f);
            float startX = pilotPos.X + ux * (edgeDist + leaderStartGap);
            float startY = pilotPos.Y + uy * (edgeDist + leaderStartGap);
            float endX = startX + ux * (pilot.LeaderLingLength * DataBlockLineLength);
            float endY = startY + uy * (pilot.LeaderLingLength * DataBlockLineLength);
            canvas.DrawLine(startX, startY, endX, endY, starsDatablockPaint);

            int mins = eramViewModel.VelocityVector;
            double distanceNm = pilot.GroundSpeed * (mins / 60.0);
            var (predLat, predLon) = DestinationPointNM(pilot.Latitude, pilot.Longitude, pilot.Heading, distanceNm);
            var predicted = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, predLat, predLon);
            SKPoint v = new SKPoint(predicted.X - pilotPos.X, predicted.Y - pilotPos.Y);
            float vLen = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
            SKPoint vvStart = vLen > 1e-3 ? new SKPoint(pilotPos.X + v.X / vLen * DataBlockLineStartOffset, pilotPos.Y + v.Y / vLen * DataBlockLineStartOffset) : pilotPos;
            canvas.DrawLine(vvStart, predicted, starsFullPaint);

            using var markFont = new SKFont(TypeFace, 12f) { Subpixel = true };
            canvas.DrawCircle(pilotPos, 8f, circlePaint);
            canvas.DrawText(mark, pilotPos.X-4, pilotPos.Y+4, markFont, outlinePaint);
            canvas.DrawText(mark, pilotPos.X - 4, pilotPos.Y + 4, markFont, starsFullPaint);

            string label = pilot.Callsign ?? string.Empty;
            float labelGap = 3f;
            float anchorX = endX + ux * labelGap;
            float anchorY = endY + uy * labelGap;
            float lx = anchorX - (bounds.Left + bounds.Width * 0.5f);
            float ly = anchorY - (bounds.Top + bounds.Height * 0.5f);
            float slx = lx;
            float sly = ly;
            float gslbx = lx;
            float gslby = ly;
            string callsign = pilot.Callsign;
            if (string.IsNullOrEmpty(cruiseAltitudeRaw)) cruiseAltitudeRaw = string.Empty;
            if (cruiseAltitudeRaw.Contains("VFR", StringComparison.OrdinalIgnoreCase))
            {
                label += "*";
            }
            font.MeasureText(label, out SKRect lb);
            font.MeasureText(pilot.Row2Col1, out SKRect slb);
            font.MeasureText(pilot.Row2Col2, out SKRect gslb);
            switch (pilot.FullDataBlockPosition)
            {
                case 1:
                    lx -= (lb.Left + lb.Width) + 2f;
                    ly -= lb.Height;

                    slx -= ((lb.Left + lb.Width) + (slb.Left + slb.Width)) + 5f;
                    sly = ly + lb.Height + 2f;

                    gslbx = (lx + lb.Right) - (gslb.Left + gslb.Width);
                    gslby = ly + lb.Height + 2f;
                    break;
                case 2:
                    lx -= (lb.Left + lb.Width) + 2f;
                    ly -= lb.Height;

                    slx -= ((lb.Left + lb.Width) + (slb.Left + slb.Width)) + 5f;
                    sly = ly + lb.Height + 2f;

                    gslbx = (lx + lb.Right) - (gslb.Left + gslb.Width);
                    gslby = ly + lb.Height + 2f;
                    break;
                case 3:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
                case 4:
                    lx -= (lb.Left + lb.Width) + 2f;
                    ly -= lb.Height;

                    slx -= ((lb.Left + lb.Width) + (slb.Left + slb.Width)) + 5f;
                    sly = ly + lb.Height + 2f;

                    gslbx = (lx + lb.Right) - (gslb.Left + gslb.Width);
                    gslby = ly + lb.Height + 2f;
                    break;
                case 5:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
                case 6:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
                case 7:
                    lx -= (lb.Left + lb.Width) + 2f;
                    ly -= lb.Height;

                    slx -= ((lb.Left + lb.Width) + (slb.Left + slb.Width)) + 5f;
                    sly = ly + lb.Height + 2f;

                    gslbx = (lx + lb.Right) - (gslb.Left + gslb.Width);
                    gslby = ly + lb.Height + 2f;
                    break;
                case 8:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
                case 9:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
            }
            canvas.DrawText(label, lx, ly, font, starsDatablockPaint);
            canvas.DrawText(pilot.Row2Col1 ?? string.Empty, slx, sly, font, starsDatablockPaint);
            canvas.DrawText(pilot.Row2Col2 ?? string.Empty, gslbx, gslby, font, starsDatablockPaint);
        }

        private void DrawStarsLimitedDatablock(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot, SKPoint pilotPos, string altitudeRaw, string altitudeLabel)
        {
            var circlePaint = new SKPaint { Color = SKColor.Parse("#1e78ff"), IsAntialias = true, Style = SKPaintStyle.Fill };
            using var outlinePaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                StrokeJoin = SKStrokeJoin.Round
            };

            const string mark = "*";
            using var font = new SKFont(TypeFace, starsLimitedDatablockPaint.TextSize) { Subpixel = true };
            font.MeasureText(mark, out SKRect bounds);
            float x = pilotPos.X - (bounds.Left + bounds.Width * 0.5f);
            float y = pilotPos.Y - (bounds.Top + bounds.Height * 0.5f);
            var (ux, uy) = GetDir(pilot.FullDataBlockPosition);
            float leaderStartGap = 2f;
            float diag = 1.4f;
            float edgeDist = (float)SquareHalfSize * ((Math.Abs(ux) > 0 && Math.Abs(uy) > 0) ? diag : 1f);
            float startX = pilotPos.X + ux * (edgeDist + leaderStartGap);
            float startY = pilotPos.Y + uy * (edgeDist + leaderStartGap);
            float endX = startX + ux * (pilot.LeaderLingLength * DataBlockLineLength);
            float endY = startY + uy * (pilot.LeaderLingLength * DataBlockLineLength);
            canvas.DrawLine(startX, startY, endX, endY, starsLimitedDatablockPaint);

            canvas.DrawCircle(pilotPos, 8f, circlePaint);
            using var markFont = new SKFont(TypeFace, 12f) { Subpixel = true };
            canvas.DrawText(mark, pilotPos.X-4f, pilotPos.Y+4f, markFont, outlinePaint);
            canvas.DrawText(mark, pilotPos.X-4f, pilotPos.Y+4f, markFont, starsLimitedPaint);

            string label = altitudeLabel ?? string.Empty;
            float labelGap = 3f;
            float anchorX = endX + ux * labelGap;
            float anchorY = endY + uy * labelGap;
            if (string.IsNullOrEmpty(altitudeLabel)) altitudeLabel = string.Empty;
            if (altitudeLabel.Contains("VFR", StringComparison.OrdinalIgnoreCase))
            {
                label = altitudeLabel.Replace("/VFR", "");
                label += "*";
            }
            font.MeasureText(label, out SKRect lb);
            float lx = anchorX - (lb.Left + lb.Width * 0.5f);
            float ly = anchorY - (lb.Top + lb.Height * 0.5f);
            switch (pilot.FullDataBlockPosition)
            {
                case 1:
                    lx -= (lb.Left + lb.Width * 0.5f) + 2f;
                    break;
                case 2:
                    lx -= (lb.Left + lb.Width * 0.5f) + 5f;
                    break;
                case 3:
                    lx += (lb.Left+lb.Width * 0.5f) + 2f;
                    break;
                case 4:
                    lx -= (lb.Left + lb.Width * 0.5f);
                    break;
                case 5:
                    lx += (lb.Left + lb.Width * 0.5f) + 2f;
                    break;
                case 6:
                    lx += (lb.Left + lb.Width * 0.5f);
                    break;
                case 7:
                    lx -= (lb.Left + lb.Width * 0.5f) + 2f;
                    break;
                case 8:
                    lx += (lb.Left + lb.Width * 0.5f) + 5f;
                    break;
                case 9:
                    lx += (lb.Left + lb.Width * 0.5f) + 2f;
                    break;
            }
            canvas.DrawText(label, lx, ly, font, starsLimitedDatablockPaint);
        }

        public void RenderHistory(SKCanvas canvas, Size size, double scale, SKPoint panOffset, Pilot pilot)
        {
            if (pilot.GroundSpeed < 30) return;
            int used = 0;
            int total = Math.Min(eramViewModel.HistoryCount, pilot.History.Count);
            int h = 0;
            if (pilot.FullDataBlock)
            {
                if (pilot.History == null)
                    return;

                for (int i = pilot.History.Count-1; i >= 0; i--)
                {
                    if (h >= total) break;

                    double lat = pilot.History[i].Lat;
                    double lon = pilot.History[i].Lon;
                    if (lat == pilot.Latitude && lon == pilot.Longitude) continue;
                    SKPoint historyPosition = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, lat, lon);

                    if (pilot.DatablockType == "ERAM")
                    {
                        var centerRect = new SKRect(historyPosition.X - SquareHalfSize, historyPosition.Y - SquareHalfSize, historyPosition.X + SquareHalfSize, historyPosition.Y + SquareHalfSize);
                        var slantStart = new SKPoint(centerRect.Left - ExtensionMargin, centerRect.Top - ExtensionMargin);
                        var slantEnd = new SKPoint(centerRect.Right + ExtensionMargin, centerRect.Bottom + ExtensionMargin);
                        canvas.DrawLine(slantStart, slantEnd, historyPaintSlant);
                    }
                    else if (pilot.DatablockType == "STARS")
                    {
                        float historySize = 3f;
                        float t = total <= 1 ? 1f : (float)h / (total - 1);

                        byte r = (byte)(30 + (18 - 30) * t);
                        byte g = (byte)(128 + (18 - 128) * t);
                        byte b = (byte)(255 + (56 - 255) * t);

                        starsHistoryPaint.Color = new SKColor(r, g, b);
                        canvas.DrawCircle(historyPosition, historySize, starsHistoryPaint);
                    }
                    h++;
                }
            }
            else
            {
                if (pilot.History == null)
                    return;

                using var starsHistoryPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
                for (int i = pilot.History.Count - 1; i >= 0; i--)
                {
                    if (h >= total) break;

                    double lat = pilot.History[i].Lat;
                    double lon = pilot.History[i].Lon;
                    SKPoint historyPosition = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, lat, lon);
                    if (lat == pilot.Latitude && lon == pilot.Longitude) continue;
                    if (pilot.DatablockType == "ERAM")
                    {
                        canvas.DrawLine(new SKPoint(historyPosition.X - HBeamSize, historyPosition.Y - IBeamSize), new SKPoint(historyPosition.X + HBeamSize, historyPosition.Y - IBeamSize), historyPaintHBeam);
                        canvas.DrawLine(new SKPoint(historyPosition.X - HBeamSize, historyPosition.Y + IBeamSize), new SKPoint(historyPosition.X + HBeamSize, historyPosition.Y + IBeamSize), historyPaintHBeam);
                        canvas.DrawLine(new SKPoint(historyPosition.X, historyPosition.Y - IBeamSize), new SKPoint(historyPosition.X, historyPosition.Y + IBeamSize), historyPaintIBeam);
                    }
                    else if (pilot.DatablockType == "STARS")
                    {
                        float historySize = 3f;
                        float t = total <= 1 ? 1f : (float)h / (total - 1);

                        byte r = (byte)(30 + (18 - 30) * t);
                        byte g = (byte)(128 + (18 - 128) * t);
                        byte b = (byte)(255 + (56 - 255) * t);

                        starsHistoryPaint.Color = new SKColor(r, g, b);
                        canvas.DrawCircle(historyPosition, historySize, starsHistoryPaint);
                    }
                    h++;
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
