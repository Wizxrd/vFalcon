using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Windows.Controls;
using vFalcon.Engines;
using vFalcon.Models;
using vFalcon.Renderables.Interfaces;
using vFalcon.Services;
using vFalcon.Utils;

namespace vFalcon.Renderables
{
    public class Renderables
    {
        private const float IBeamSize = 6.5f;
        private const float HBeamSize = 5.5f;
        private const float TargetSize = 3.5f;
        private const float CorrelatedSlantSize = 2f;
        private const float VelcityVectorStartOffset = 5f;
        private const float DatablockLeaderLineLength = 45f;
        private const float StarsDatablockLeaderLineLength = 25f;
        private const float DatablockLeaderLineOffset = 2f;
        private const float DatablockXOffset = 5f;
        private const float DatablockYSpacing = 2f;
        private const float DwellEmphasisPadding = 2f;
        private const float StarsTargetSize = 8f;

        public static List<IRenderable> FromFeatures(DisplayState displayState, IEnumerable<ProcessedFeature> features)
        {
            var list = new List<IRenderable>();
            foreach (var f in features)
            {
                var geomType = f.GeometryType == "Line" ? "LineString" : f.GeometryType;
                var geomObj = f.Geometry as JObject ?? (f.Geometry is JToken t ? (JObject)t : null);
                if (geomObj == null) continue;

                var coords = geomObj["coordinates"] as JArray;
                if (coords == null) continue;

                var style = (f.AppliedAttributes.TryGetValue("style", out var sv) && sv != null) ? sv.ToString() : "OtherWaypoints";
                var rgb = (byte)(App.Profile.AppearanceSettings.MapBrightness * 243 / 100);

                switch (geomType)
                {
                    case "LineString":
                        {
                            var segments = ToSegments(geomType, coords);
                            if (segments.Count == 0) break;
                            SKPaint paint = Paint.VideoMapLine(f, rgb);
                            foreach (var segment in segments)
                            {
                                if (segment.Count < 2) continue;
                                for (int i = 0; i < segment.Count - 1; i++)
                                {
                                    SKPoint startScreen = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, segment[i].lat, segment[i].lon);
                                    SKPoint endScreen = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, segment[i + 1].lat, segment[i + 1].lon);
                                    list.Add(new Line(startScreen, endScreen, paint, 0));
                                }
                            }
                            break;
                        }
                    case "MultiLineString":
                        {
                            var segments = ToSegments(geomType, coords);
                            if (segments.Count == 0) break;
                            SKPaint paint = Paint.VideoMapLine(f, rgb);
                            foreach (var segment in segments)
                            {
                                if (segment.Count < 2) continue;
                                for (int i = 0; i < segment.Count - 1; i++)
                                {
                                    SKPoint startScreen = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, segment[i].lat, segment[i].lon);
                                    SKPoint endScreen = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, segment[i + 1].lat, segment[i + 1].lon);
                                    list.Add(new Line(startScreen, endScreen, paint, 0));
                                }
                            }
                            break;
                        }
                    case "Point":
                        {
                            double lon = coords[0].Value<double>();
                            double lat = coords[1].Value<double>();
                            SKPaint paint = Paint.VideoMapSymbol(rgb);
                            if (lat != 0 || lon != 0)
                            {
                                SKPoint screenPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, lat, lon);
                                list.Add(new Symbol(style, screenPoint, paint, 0));
                            }
                            break;
                        }
                    case "Text":
                        {
                            double lon = coords[0].Value<double>();
                            double lat = coords[1].Value<double>();
                            if (lat != 0 || lon != 0)
                            {
                                SKPaint paint = Paint.VideoMapText(rgb, f.FontSize);
                                SKPoint screenPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, lat, lon);
                                SKPoint drawPoint = new SKPoint(screenPoint.X + f.XOffset, screenPoint.Y - (f.YOffset * 1.5f));
                                list.Add(new Text(f.TextContent, drawPoint, paint, 0));
                            }
                            break;
                        }
                }
            }
            return list;
        }

        public static List<IRenderable> FromPilots(DisplayState displayState, Dictionary<string, Pilot> pilots)
        {
            List<IRenderable> renderables = new();
            foreach (Pilot pilot in pilots.Values.ToList())
            {
                if (pilot == null) continue;
                if (pilot.GroundSpeed < 30)
                {
                    if (!App.Profile.TopDown) continue;
                    renderables.AddRange(ForTopDown(displayState, pilot));
                    continue;
                }
                if (pilot.DisplayFiledRoute)
                {
                    renderables.AddRange(ForFiledRoute(displayState, pilot));
                }
                switch (pilot.DatablockType)
                {
                    case DatablockType.Eram:
                        {
                            renderables.AddRange(ForEram(displayState, pilot));
                            break;
                        }
                    case DatablockType.Stars:
                        {
                            renderables.AddRange(ForStars(displayState, pilot));
                            break;
                        }
                    default: break;
                }
            }
            return renderables;
        }

        private static List<IRenderable> ForFiledRoute(DisplayState displayState, Pilot pilot)
        {
            List<IRenderable> renderables = new();
            string route = $"{pilot.FlightPlan?["departure"]} {pilot.FlightPlan?["route"]} {pilot.FlightPlan?["arrival"]}";
            route = string.Join(" ", App.MainWindowViewModel.RouteService.GetFixesFromRouteString(route));
            string nextFix = App.MainWindowViewModel.RouteService.GetNextFix(route, pilot.Heading, pilot.Latitude, pilot.Longitude);
            string routeAfterFix = App.MainWindowViewModel.RouteService.GetRouteAfterFix(route, nextFix);
            JArray fixesFromRoute = App.MainWindowViewModel.RouteService.GetCoordsFromRouteString(routeAfterFix);
            SKPoint pilotPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, pilot.Latitude, pilot.Longitude);
            SKPoint nextFixPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, (double)fixesFromRoute[0][0], (double)fixesFromRoute[0][1]);
            SKColor paintColor = new();
            if (pilot.DatablockType == DatablockType.Eram) paintColor = Colors.Yellow;
            else if (pilot.DatablockType == DatablockType.Stars) paintColor = Colors.Blue;
            renderables.Add(new Line(pilotPoint, nextFixPoint, Paint.Line(paintColor, SKPaintStyle.Stroke, 1f), 1));
            renderables.Add(new Text(pilot.Callsign, new SKPoint(pilotPoint.X-25, pilotPoint.Y + 40), Paint.Text(paintColor, 18f, SKPaintStyle.Fill), 1));
            for (int i = 0; i < fixesFromRoute.Count; i++)
            {
                var firstCoordinate = fixesFromRoute[i];
                double firstLat = (double)firstCoordinate[0];
                double firstLon = (double)firstCoordinate[1];
                SKPoint firstPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, firstLat, firstLon);
                if (i != fixesFromRoute.Count - 1)
                {
                    var nextCoordinate = fixesFromRoute[i+1];
                    double nextLat = (double)nextCoordinate[0];
                    double nextLon = (double)nextCoordinate[1];
                    SKPoint nextPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, nextLat, nextLon);
                    renderables.Add(new Line(firstPoint, nextPoint, Paint.Line(paintColor, SKPaintStyle.Stroke, 1f), 1));
                }
                else
                {
                    renderables.Add(new Symbol("routeEnd", firstPoint, Paint.Symbol(paintColor, SKPaintStyle.Stroke, 1f), 1));
                }
            }
            return renderables;
        }

        private static List<IRenderable> ForTopDown(DisplayState displayState, Pilot pilot)
        {
            List<IRenderable> renderables = new();
            SKPoint pilotScreenPoint = GetPilotScreenPoint(displayState, pilot);
            SKPaint circlePaint = Paint.Circle(SKColors.White, SKPaintStyle.Fill, 0);
            SKPaint textPaint = Paint.Text(SKColors.LimeGreen, 12, SKPaintStyle.Fill);
            renderables.Add(new Circle(pilotScreenPoint, 3, circlePaint, 1));

            string line1Text = pilot.Callsign;
            string line2Text = (string)pilot.FlightPlan?["aircraft_short"] ?? string.Empty;

            SKPoint line1Point = new SKPoint(pilotScreenPoint.X + 10, pilotScreenPoint.Y - 10);
            float line1Height = SkiaEngine.GetTextHeight(textPaint);
            SKPoint lin2Point = new SKPoint(pilotScreenPoint.X + 10, line1Point.Y + (line1Height + DatablockYSpacing));
            renderables.Add(new Text(line1Text, line1Point, textPaint, 1));
            renderables.Add(new Text(line2Text, lin2Point, textPaint, 1));
            return renderables;
        }

        private static List<IRenderable> ForEram(DisplayState displayState, Pilot pilot)
        {
            List<IRenderable> renderables = new();
            if (pilot.FullDatablockEnabled)
            {
                renderables.AddRange(ForEramFullDatablock(displayState, pilot));
                renderables.AddRange(ForEramFullDatablockHistory(displayState, pilot));
            }
            else
            {
                renderables.AddRange(ForEramLimitedDatablock(displayState, pilot));
                renderables.AddRange(ForEramLimitedDatablockHistory(displayState, pilot));
            }
            return renderables;
        }

        private static List<IRenderable> ForStars(DisplayState displayState, Pilot pilot)
        {
            List<IRenderable> renderables = new();
            if (pilot.FullDatablockEnabled)
            {
                renderables.AddRange(ForStarsFullDatablock(displayState, pilot));
            }
            else
            {
                renderables.AddRange(ForStarsLimitedDatablock(displayState, pilot));
            }
            renderables.AddRange(ForStarsHistory(displayState, pilot));
            return renderables;
        }

        private static List<IRenderable> ForEramFullDatablock(DisplayState displayState, Pilot pilot)
        {
            var (rectPaint, linePaint, textPaint, circlePaint) = CreateEramPaints();
            var (pilotScreenPoint, targetBox, slantStart, slantEnd) = PilotScreenAndBox(displayState, pilot);

            var renderables = new List<IRenderable>
            {
                new Rect(targetBox, rectPaint, 2),
                new Line(slantStart, slantEnd, rectPaint, 2)
            };

            var (velocityStart, velocityEnd) = VelocityVector(displayState, pilot, pilotScreenPoint);
            renderables.Add(new Line(velocityStart, velocityEnd, rectPaint, 2));

            var (leaderStart, leaderEnd) = LeaderLine(pilot, targetBox);
            renderables.Add(new Line(leaderStart, leaderEnd, linePaint, 2));

            var (line1Text, line2Text, line3Text, line4Text) = DatablockTexts(pilot);
            var (datablockWidth, lineHeight) = MeasureDatablock(textPaint, line1Text, line2Text, line3Text, line4Text);
            var datablockOrigin = PositionDatablockOrigin(pilot, leaderEnd, datablockWidth, lineHeight);
            var (line1Point, line2Point, line3Point, line4Point) = DatablockLinePositions(datablockOrigin, lineHeight);

            renderables.Add(new Text(line1Text, line1Point, textPaint, 2));
            renderables.Add(new Text(line2Text, line2Point, textPaint, 2));
            renderables.Add(new Text(line3Text, line3Point, textPaint, 2));
            renderables.Add(new Text(line4Text, line4Point, textPaint, 2));
            if (pilot.DwellEmphasisEnabled)
            {
                SKRect rect = DwellEmphasis(textPaint, rectPaint, line1Point, line4Point, datablockWidth);
                renderables.Add(new Rect(rect, rectPaint, 1));
            }
            if (pilot.DriEnabled)
            {
                float radius = GetDistanceReferenceIndicatorRadius(displayState.Scale, pilot.Latitude, pilot.DriSize);
                var (text, point) = GetDistanceReferenceIndicator(textPaint, pilot.DatablockPosition, pilotScreenPoint, radius, pilot.DriSize);
                renderables.Add(new Circle(pilotScreenPoint, radius, circlePaint, 1));
                renderables.Add(new Text(text, point, circlePaint, 1));
            }
            return renderables;
        }
        
        public static List<IRenderable> ForEramFullDatablockHistory(DisplayState displayState, Pilot pilot)
        {
            List<IRenderable> renderables = new List<IRenderable>();
            int total = Math.Min(App.Profile.AppearanceSettings.HistoryLength, pilot.History.Count);
            int h = 0;
            SKPaint linePaint = Paint.Line(Colors.EramFullDatablockHistory, SKPaintStyle.Stroke, 1f);
            for (int i = pilot.History.Count - 1; i >= 0; i--)
            {
                if (h >= total) break;
                if (i == pilot.History.Count - 1) continue; //FIXME
                double lat = pilot.History[i].Lat;
                double lon = pilot.History[i].Lon;
                if (lat == pilot.Latitude && lon == pilot.Longitude) continue;
                SKPoint historyPosition = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, lat, lon);
                var centerRect = new SKRect(historyPosition.X - TargetSize, historyPosition.Y - TargetSize, historyPosition.X + TargetSize, historyPosition.Y + TargetSize);
                var slantStart = new SKPoint(centerRect.Left - CorrelatedSlantSize, centerRect.Top - CorrelatedSlantSize);
                var slantEnd = new SKPoint(centerRect.Right + CorrelatedSlantSize, centerRect.Bottom + CorrelatedSlantSize);
                renderables.Add(new Line(slantStart, slantEnd, linePaint, 1));
                h++;
            }
            return renderables;
        }

        private static float GetDistanceReferenceIndicatorRadius(double scale, double latitude, float size)
        {
            double sKm = size * 1.852;
            double cosPhi = Math.Cos(latitude * ScreenMap.RadPerDeg);
            if (cosPhi < 1e-6) cosPhi = 1e-6;

            return (float)((sKm / cosPhi) * scale);
        }

        private static (string text, SKPoint point) GetDistanceReferenceIndicator(SKPaint textPaint, DatablockPosition datablockPosition, SKPoint pilotScreenPoint, float radius, int driSize)
        {
            var posOpp = GetOppositePosition(datablockPosition);
            var (ux, uy) = GetPositionModifier(posOpp);

            float insideOffset = 15f;
            if (posOpp == DatablockPosition.SouthEast || posOpp == DatablockPosition.Default || posOpp == DatablockPosition.East || posOpp == DatablockPosition.NorthEast)
            {
                insideOffset = 15f;
            }
            else if (posOpp == DatablockPosition.SouthWest || posOpp == DatablockPosition.NorthWest)
            {
                insideOffset = 30f;
            }
            else if (posOpp == DatablockPosition.West)
            {
                insideOffset = 40f;
            }

            float px = pilotScreenPoint.X + ux * (radius + insideOffset);
            float py = pilotScreenPoint.Y + uy * (radius + insideOffset);

            float baselineY = py - SkiaEngine.GetTextHeight(textPaint);
            return ($"{driSize}NM", new SKPoint(px, baselineY));
        }

        private static List<IRenderable> ForEramLimitedDatablock(DisplayState displayState, Pilot pilot)
        {
            List<IRenderable> renderables = new();
            double pilotLat = pilot.Latitude;
            double pilotLon = pilot.Longitude;
            string filedCruise = pilot.FlightPlan?["altitude"]?.ToString();
            string currentAltitude = (pilot.Altitude / 100).ToString("D3");
            SKPoint pilotPos = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, pilotLat, pilotLon);
            SKPaint textPaint = Paint.Text(Colors.EramLimitedDatablock, App.Profile.AppearanceSettings.DatablockFontSize, SKPaintStyle.Fill);
            SKPaint linePaint = Paint.Line(Colors.Yellow, SKPaintStyle.Stroke, 1f);
            float callsignBaseline = pilotPos.Y + IBeamSize;
            string callsign = pilot.Callsign ?? string.Empty;
            float callsignX = pilotPos.X + 15f;

            float lineHeight = SkiaEngine.GetTextHeight(textPaint);
            float lineGap = 2f;

            float callsignW = SkiaEngine.MeasureText(textPaint, callsign);
            renderables.Add(new Text(callsign, new SKPoint(callsignX, callsignBaseline), textPaint, 2));
            renderables.Add(new Line(new SKPoint(pilotPos.X - HBeamSize, pilotPos.Y - IBeamSize), new SKPoint(pilotPos.X + HBeamSize, pilotPos.Y - IBeamSize), linePaint, 2));
            renderables.Add(new Line(new SKPoint(pilotPos.X - HBeamSize, pilotPos.Y + IBeamSize), new SKPoint(pilotPos.X + HBeamSize, pilotPos.Y + IBeamSize), linePaint, 2));
            renderables.Add(new Line(new SKPoint(pilotPos.X, pilotPos.Y - IBeamSize), new SKPoint(pilotPos.X, pilotPos.Y + IBeamSize), linePaint, 2));

            float altitudeBaseline = callsignBaseline + lineHeight + lineGap;
            float altitudeW = SkiaEngine.MeasureText(textPaint, currentAltitude);
            renderables.Add(new Text(currentAltitude, new SKPoint(callsignX, altitudeBaseline), textPaint, 1));

            if (pilot.DwellEmphasisEnabled)
            {
                SKPaint rectPaint = Paint.Rect(Colors.Yellow, SKPaintStyle.Stroke, 1f);
                SKRect rect = DwellEmphasis(textPaint, rectPaint, new SKPoint(callsignX, callsignBaseline), new SKPoint(callsignX, altitudeBaseline), SkiaEngine.MeasureText(textPaint, callsign));
                renderables.Add(new Rect(rect, rectPaint, 1));
            }
            if (pilot.DriEnabled)
            {
                SKPaint circlePaint = Paint.Circle(Colors.Yellow, SKPaintStyle.Stroke, 1f);
                float radius = GetDistanceReferenceIndicatorRadius(displayState.Scale, pilot.Latitude, pilot.DriSize);
                var (text, point) = GetDistanceReferenceIndicator(textPaint, pilot.DatablockPosition, pilotPos, radius, pilot.DriSize);
                renderables.Add(new Circle(pilotPos, radius, circlePaint, 1));
                renderables.Add(new Text(text, point, circlePaint, 1));
            }
            return renderables;
        }

        private static List<IRenderable> ForEramLimitedDatablockHistory(DisplayState displayState, Pilot pilot)
        {
            List<IRenderable> renderables = new List<IRenderable>();
            int total = Math.Min(App.Profile.AppearanceSettings.HistoryLength, pilot.History.Count);
            int h = 0;
            double pilotLat = pilot.Latitude;
            double pilotLon = pilot.Longitude;
            SKPaint linePaint = Paint.Line(Colors.EramLimitedDatablockHistory, SKPaintStyle.Stroke, 1f);
            for (int i = pilot.History.Count - 1; i >= 0; i--)
            {
                if (h >= total) break;
                if (i == pilot.History.Count - 1) continue; //FIXME
                double lat = pilot.History[i].Lat;
                double lon = pilot.History[i].Lon;
                if (lat == pilot.Latitude && lon == pilot.Longitude) continue;
                SKPoint historyPosition = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, lat, lon);
                renderables.Add(new Line(new SKPoint(historyPosition.X - HBeamSize, historyPosition.Y - IBeamSize), new SKPoint(historyPosition.X + HBeamSize, historyPosition.Y - IBeamSize), linePaint, 1));
                renderables.Add(new Line(new SKPoint(historyPosition.X - HBeamSize, historyPosition.Y + IBeamSize), new SKPoint(historyPosition.X + HBeamSize, historyPosition.Y + IBeamSize), linePaint, 1));
                renderables.Add(new Line(new SKPoint(historyPosition.X, historyPosition.Y - IBeamSize), new SKPoint(historyPosition.X, historyPosition.Y + IBeamSize), linePaint, 1));
                h++;
            }
            return renderables;
        }

        private static (SKPoint startPoint, SKPoint endPoint) GetLeaderLine(SKPoint pilotScreenPoint, Pilot pilot, float ux, float uy)
        {
            float leaderStartGap = 1f;
            float diag = 1f;
            float edgeDist = (float)StarsTargetSize * ((Math.Abs(ux) > 0 && Math.Abs(uy) > 0) ? diag : 1f);
            float startX = pilotScreenPoint.X + ux * (edgeDist + leaderStartGap);
            float startY = pilotScreenPoint.Y + uy * (edgeDist + leaderStartGap);
            float endX = startX + ux * (pilot.LeaderLineLength * StarsDatablockLeaderLineLength);
            float endY = startY + uy * (pilot.LeaderLineLength * StarsDatablockLeaderLineLength);
            SKPoint startPoint = new SKPoint(startX, startY);
            SKPoint endPoint = new SKPoint(endX, endY);
            return (startPoint, endPoint);
        }

        private static List<IRenderable> ForStarsLimitedDatablock(DisplayState displayState, Pilot pilot)
            => ForStarsDatablock(displayState, pilot, Colors.StarsLimitedDatablock, Colors.LimeGreen);

        private static List<IRenderable> ForStarsFullDatablock(DisplayState displayState, Pilot pilot)
            => ForStarsDatablock(displayState, pilot, Colors.StarsFullDatablock, Colors.White);
        private static List<IRenderable> ForStarsDatablock(DisplayState displayState, Pilot pilot, SKColor styleColor, SKColor markColor)
        {
            List<IRenderable> renderables = new();
            SKPaint textPaint = Paint.Text(styleColor, App.Profile.AppearanceSettings.DatablockFontSize, SKPaintStyle.Fill);
            SKPaint leaderLinePaint = Paint.Line(styleColor, SKPaintStyle.Stroke, 1f);
            SKPaint circlePaint = Paint.Circle(Colors.Blue, SKPaintStyle.Fill, 0);
            SKPoint pilotScreenPoint = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, pilot.Latitude, pilot.Longitude);
            renderables.Add(new Circle(pilotScreenPoint, StarsTargetSize, circlePaint, 2));
            var (ux, uy) = GetPositionModifier(pilot.DatablockPosition);
            (SKPoint startPoint, SKPoint endPoint) = GetLeaderLine(pilotScreenPoint, pilot, ux, uy);
            renderables.Add(new Line(startPoint, endPoint, leaderLinePaint, 2));

            SKPaint markPaint = Paint.Text(markColor, 12f, SKPaintStyle.Fill);
            SKPaint markHighlightPaint = Paint.Text(SKColors.Black, 12f, SKPaintStyle.Stroke, 2f);
            string mark = "*";
            float textWidth = SkiaEngine.MeasureText(markPaint, mark);
            markPaint.ToFont().MeasureText(mark, out SKRect bounds);
            float x = pilotScreenPoint.X - (bounds.Left + bounds.Width * 0.5f);
            float y = pilotScreenPoint.Y - (bounds.Top + bounds.Height * 0.5f);
            SKPoint p = new SKPoint(x, y);
            renderables.Add(new Text(mark, p, markHighlightPaint, 2));
            renderables.Add(new Text(mark, p, markPaint, 3));

            float labelGap = 1f;
            float anchorX = endPoint.X + ux * labelGap;
            float anchorY = endPoint.Y + uy * labelGap;
            float lx = anchorX - (bounds.Left + bounds.Width * 0.5f);
            float ly = anchorY - (bounds.Top + bounds.Height * 0.5f);
            float slx = lx;
            float sly = ly;
            float gslbx = lx;
            float gslby = ly;
            string callsign = pilot.Callsign;
            textPaint.ToFont().MeasureText(pilot.Callsign, out SKRect lb);
            textPaint.ToFont().MeasureText(pilot.StarsDatablockRow2Col1, out SKRect slb);
            textPaint.ToFont().MeasureText(pilot.StarsDatablockRow2Col2, out SKRect gslb);
            switch (pilot.DatablockPosition)
            {
                case DatablockPosition.SouthWest:
                    lx -= (lb.Left + lb.Width) + 2f;
                    ly -= lb.Height;
                    slx -= ((lb.Left + lb.Width) + (slb.Left + slb.Width)) + 5f;
                    sly = ly + lb.Height + 2f;

                    gslbx = (lx + lb.Right) - (gslb.Left + gslb.Width);
                    gslby = ly + lb.Height + 2f;
                    break;
                case DatablockPosition.South:
                    lx -= (lb.Left + lb.Width) + 2f;
                    ly -= lb.Height;

                    slx -= ((lb.Left + lb.Width) + (slb.Left + slb.Width)) + 5f;
                    sly = ly + lb.Height + 2f;

                    gslbx = (lx + lb.Right) - (gslb.Left + gslb.Width);
                    gslby = ly + lb.Height + 2f;
                    break;
                case DatablockPosition.SouthEast:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
                case DatablockPosition.West:
                    lx -= (lb.Left + lb.Width) + 2f;
                    ly -= lb.Height;

                    slx -= ((lb.Left + lb.Width) + (slb.Left + slb.Width)) + 5f;
                    sly = ly + lb.Height + 2f;

                    gslbx = (lx + lb.Right) - (gslb.Left + gslb.Width);
                    gslby = ly + lb.Height + 2f;
                    break;
                case DatablockPosition.Default:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
                case DatablockPosition.East:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
                case DatablockPosition.NorthWest:
                    lx -= (lb.Left + lb.Width) + 2f;
                    ly -= lb.Height;

                    slx -= ((lb.Left + lb.Width) + (slb.Left + slb.Width)) + 5f;
                    sly = ly + lb.Height + 2f;

                    gslbx = (lx + lb.Right) - (gslb.Left + gslb.Width);
                    gslby = ly + lb.Height + 2f;
                    break;
                case DatablockPosition.North:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
                case DatablockPosition.NorthEast:
                    lx += 7f;
                    ly -= lb.Height;

                    slx += 7f;
                    sly = ly + lb.Height + 2f;

                    gslbx = slx + (slb.Left + slb.Width) + 10f;
                    gslby = ly + lb.Height + 2f;
                    break;
            }
            renderables.Add(new Text(pilot.Callsign, new SKPoint(lx, ly), textPaint, 2));
            renderables.Add(new Text(pilot.StarsDatablockRow2Col1, new SKPoint(slx, sly), textPaint, 2));
            renderables.Add(new Text(pilot.StarsDatablockRow2Col2, new SKPoint(gslbx, gslby), textPaint, 2));

            if (pilot.DriEnabled)
            {
                SKPaint driPaint = Paint.Circle(Colors.Blue, SKPaintStyle.Stroke, 1f);
                SKPaint driTextPaint = Paint.Text(Colors.Blue, 12f, SKPaintStyle.Fill);
                float radius = GetDistanceReferenceIndicatorRadius(displayState.Scale, pilot.Latitude, pilot.DriSize);
                var (text, point) = GetDistanceReferenceIndicator(textPaint, pilot.DatablockPosition, pilotScreenPoint, radius, pilot.DriSize);
                renderables.Add(new Circle(pilotScreenPoint, radius, driPaint, 1));
                renderables.Add(new Text(text, point, driTextPaint, 1));
            }

            if (pilot.FullDatablockEnabled)
            {
                SKPaint linePaint = Paint.Line(styleColor, SKPaintStyle.Stroke, 1f);
                var (velocityStart, velocityEnd) = VelocityVector(displayState, pilot, pilotScreenPoint);
                renderables.Add(new Line(velocityStart, velocityEnd, linePaint, 1));
            }

            return renderables;
        }

        private static List<IRenderable> ForStarsHistory(DisplayState displayState, Pilot pilot)
        {
            List<IRenderable> renderables = new();
            int total = Math.Min(App.Profile.AppearanceSettings.HistoryLength, pilot.History.Count);
            int h = 0;
            double pilotLat = pilot.Latitude;
            double pilotLon = pilot.Longitude;
            for (int i = pilot.History.Count - 1; i >= 0; i--)
            {
                if (h >= total) break;
                double lat = pilot.History[i].Lat;
                double lon = pilot.History[i].Lon;
                if (lat == pilot.Latitude && lon == pilot.Longitude) continue;
                SKPoint historyPosition = ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, lat, lon);
                float t = total <= 1 ? 1f : (float)h / (total - 1);
                byte r = (byte)(30 + (18 - 30) * t);
                byte g = (byte)(128 + (18 - 128) * t);
                byte b = (byte)(255 + (56 - 255) * t);
                SKPaint circlePaint = Paint.Line(SKColors.Red, SKPaintStyle.Fill, 0);
                circlePaint.Color = SkiaEngine.ScaleColor(new SKColor(r, g, b), App.Profile.AppearanceSettings.HistoryBrightness*2.55);
                renderables.Add(new Circle(historyPosition, StarsTargetSize/2, circlePaint, 1));
                h++;
            }
            return renderables;
        }

        private static SKRect DwellEmphasis(SKPaint textPaint, SKPaint rectPaint, SKPoint line1BaselinePoint, SKPoint line4BaselinePoint, float datablockWidth)
        {
            var metrics = textPaint.FontMetrics;

            float left = line1BaselinePoint.X;
            float right = left + datablockWidth;
            float top = line1BaselinePoint.Y + metrics.Top;
            float bottom = line4BaselinePoint.Y + metrics.Bottom;

            return new SKRect(
                left - DwellEmphasisPadding,
                top - DwellEmphasisPadding,
                right + DwellEmphasisPadding,
                bottom + DwellEmphasisPadding
            );
        }

        private static (float ux, float uy) GetPositionModifier(DatablockPosition pos)
        {
            return pos switch
            {
                DatablockPosition.SouthWest => (-0.7071f, 0.7071f),
                DatablockPosition.South => (0.0f, 1.0f),
                DatablockPosition.SouthEast => (0.7071f, 0.7071f),
                DatablockPosition.West => (-1.0f, 0.0f),
                DatablockPosition.Default => (0.7071f, -0.7071f),
                DatablockPosition.East => (1.0f, 0.0f),
                DatablockPosition.NorthWest => (-0.7071f, -0.7071f),
                DatablockPosition.North => (0.0f, -1.0f),
                DatablockPosition.NorthEast => (0.7071f, -0.7071f),
                _ => (0.0f, 1.0f),
            };
        }

        private static DatablockPosition GetOppositePosition(DatablockPosition position) => position switch
        {
            DatablockPosition.SouthWest => DatablockPosition.NorthEast,
            DatablockPosition.South => DatablockPosition.North,
            DatablockPosition.SouthEast => DatablockPosition.NorthWest,
            DatablockPosition.West => DatablockPosition.East,
            DatablockPosition.Default => DatablockPosition.SouthWest,
            DatablockPosition.East => DatablockPosition.West,
            DatablockPosition.NorthWest => DatablockPosition.SouthEast,
            DatablockPosition.North => DatablockPosition.South,
            DatablockPosition.NorthEast => DatablockPosition.SouthWest,
            _ => DatablockPosition.Default
        };

        private static (SKPaint rectPaint, SKPaint linePaint, SKPaint textPaint, SKPaint circlePaint) CreateEramPaints()
        {
            var rectPaint = Paint.Rect(Colors.Yellow, SKPaintStyle.Stroke, 1f);
            var linePaint = Paint.Line(Colors.EramFullDatablock, SKPaintStyle.Stroke, 1f);
            var textPaint = Paint.Text(Colors.EramFullDatablock, App.Profile.AppearanceSettings.DatablockFontSize, SKPaintStyle.Fill);
            var circlePaint = Paint.Circle(Colors.Yellow, SKPaintStyle.Stroke, 1f);
            return (rectPaint, linePaint, textPaint, circlePaint);
        }

        private static (SKPoint pilotScreenPoint, SKRect targetBox, SKPoint slantStart, SKPoint slantEnd) PilotScreenAndBox(DisplayState s, Pilot p) // FIXME
        {
            var pilotScreenPoint = GetPilotScreenPoint(s, p);
            var targetBox = new SKRect(pilotScreenPoint.X - TargetSize, pilotScreenPoint.Y - TargetSize,
                                       pilotScreenPoint.X + TargetSize, pilotScreenPoint.Y + TargetSize);
            var slantStart = new SKPoint(targetBox.Left - CorrelatedSlantSize, targetBox.Top - CorrelatedSlantSize);
            var slantEnd = new SKPoint(targetBox.Right + CorrelatedSlantSize, targetBox.Bottom + CorrelatedSlantSize);
            return (pilotScreenPoint, targetBox, slantStart, slantEnd);
        }

        private static SKPoint GetPilotScreenPoint(DisplayState displayState, Pilot pilot)
        {
            return ScreenMap.CoordinateToScreen(displayState.Width, displayState.Height, displayState.Scale, displayState.PanOffset, pilot.Latitude, pilot.Longitude);
        }

        private static (SKPoint velocityStart, SKPoint velocityEnd) VelocityVector(DisplayState s, Pilot p, SKPoint pilotScreenPoint)
        {
            int vectorMinutes = App.Profile.AppearanceSettings.VelocityVector;
            double vectorDistanceNm = p.GroundSpeed * (vectorMinutes / 60.0);
            var projected = ScreenMap.ProjectPointOnBearing(p.Latitude, p.Longitude, p.Heading, vectorDistanceNm);
            var predictedScreenPoint = ScreenMap.CoordinateToScreen(s.Width, s.Height, s.Scale, s.PanOffset, projected.Lat, projected.Lon);

            var delta = new SKPoint(predictedScreenPoint.X - pilotScreenPoint.X, predictedScreenPoint.Y - pilotScreenPoint.Y);
            float deltaLength = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            var velocityStart = deltaLength > 1e-3
                ? new SKPoint(pilotScreenPoint.X + delta.X / deltaLength * VelcityVectorStartOffset,
                              pilotScreenPoint.Y + delta.Y / deltaLength * VelcityVectorStartOffset)
                : pilotScreenPoint;

            var velocityEnd = predictedScreenPoint;
            return (velocityStart, velocityEnd);
        }

        private static (SKPoint leaderStart, SKPoint leaderEnd) LeaderLine(Pilot p, SKRect targetBox)
        {
            var (ux, uy) = GetPositionModifier(p.DatablockPosition);
            float diagMultiplier = 1.4f;
            float edgeDistance = (float)TargetSize * ((Math.Abs(ux) > 0 && Math.Abs(uy) > 0) ? diagMultiplier : 1f);

            float leaderStartX = targetBox.MidX + ux * (edgeDistance + DatablockLeaderLineOffset);
            float leaderStartY = targetBox.MidY + uy * (edgeDistance + DatablockLeaderLineOffset);
            float leaderEndX = leaderStartX + ux * (p.LeaderLineLength * DatablockLeaderLineLength);
            float leaderEndY = leaderStartY + uy * (p.LeaderLineLength * DatablockLeaderLineLength);

            var leaderStart = new SKPoint(leaderStartX, leaderStartY);
            var leaderEnd = new SKPoint(leaderEndX, leaderEndY);
            return (leaderStart, leaderEnd);
        }

        private static (string line1Text, string line2Text, string line3Text, string line4Text) DatablockTexts(Pilot p)
        {
            string line1Text = p.Callsign;

            string filedCruise = p.FlightPlan?["altitude"]?.ToString() ?? "XXXXX";
            string line2Text = (p.Altitude / 100).ToString("D3");
            if (int.TryParse(filedCruise, out int cruiseAltitude) && cruiseAltitude != 0)
            {
                string cruiseText = (cruiseAltitude / 100).ToString("D3");
                if (Math.Abs(p.Altitude - cruiseAltitude) <= 500) line2Text = cruiseText + "C";
                else if (p.Altitude < cruiseAltitude) line2Text = $"{cruiseText}\u2191{line2Text}";
                else line2Text = $"{cruiseText}\u2193{line2Text}";
            }

            string aircraftType = p.FlightPlan?["aircraft_short"]?.ToString() ?? "XXXX";
            string line3Text = $"{aircraftType} {p.GroundSpeed}";
            string line4Text = p.FlightPlan?["arrival"]?.ToString() ?? "XXXX";

            return (line1Text, line2Text, line3Text, line4Text);
        }

        private static (float datablockWidth, float lineHeight) MeasureDatablock(SKPaint textPaint, string line1Text, string line2Text, string line3Text, string line4Text)
        {
            float lineHeight = SkiaEngine.GetTextHeight(textPaint);

            float maxWidth = Math.Max(Math.Max(SkiaEngine.MeasureText(textPaint, line1Text), SkiaEngine.MeasureText(textPaint, line2Text)),
                                      Math.Max(SkiaEngine.MeasureText(textPaint, line3Text), SkiaEngine.MeasureText(textPaint, line4Text)));

            return (maxWidth, lineHeight);
        }

        private static SKPoint PositionDatablockOrigin(Pilot p, SKPoint leaderEndPoint, float datablockWidth, float lineHeight)
        {
            var origin = leaderEndPoint;
            var pos = p.DatablockPosition;

            if (pos == DatablockPosition.Default || pos == DatablockPosition.North || pos == DatablockPosition.NorthEast)
                origin.X += DatablockXOffset;
            else if (pos == DatablockPosition.NorthWest)
                origin.X -= datablockWidth + DatablockXOffset;
            else if (pos == DatablockPosition.South || pos == DatablockPosition.SouthEast)
            {
                origin.X += DatablockXOffset;
                origin.Y += (lineHeight + DatablockYSpacing) * 2;
            }
            else if (pos == DatablockPosition.SouthWest)
            {
                origin.X -= datablockWidth + DatablockXOffset;
                origin.Y += (lineHeight + DatablockYSpacing) * 2;
            }
            else if (pos == DatablockPosition.East)
            {
                origin.X += DatablockXOffset;
                origin.Y += (lineHeight - DatablockYSpacing) / 2;
            }
            else if (pos == DatablockPosition.West)
            {
                origin.X -= datablockWidth + DatablockXOffset;
                origin.Y += (lineHeight - DatablockYSpacing) / 2;
            }

            return origin;
        }

        private static (SKPoint line1Point, SKPoint line2Point, SKPoint line3Point, SKPoint line4Point) DatablockLinePositions(SKPoint datablockOrigin, float lineHeight)
        {
            var line1Point = datablockOrigin; line1Point.Y -= (lineHeight + DatablockYSpacing) * 2;
            var line2Point = datablockOrigin; line2Point.Y -= lineHeight + DatablockYSpacing;
            var line3Point = datablockOrigin;
            var line4Point = datablockOrigin; line4Point.Y += lineHeight + DatablockYSpacing;
            return (line1Point, line2Point, line3Point, line4Point);
        }

        private static List<List<(double lat, double lon)>> ToSegments(string geomType, JArray coords)
        {
            var result = new List<List<(double lat, double lon)>>();
            if (geomType == "LineString")
            {
                var s = ParseLine(coords);
                if (s.Count > 0) result.Add(s);
            }
            else
            {
                foreach (var item in coords)
                {
                    if (item is JArray sub)
                    {
                        if (sub.First is JArray inner && inner.First is JArray)
                            result.AddRange(ToSegments("MultiLineString", sub));
                        else
                        {
                            var s = ParseLine(sub);
                            if (s.Count > 0) result.Add(s);
                        }
                    }
                }
            }
            return result;
        }

        private static List<(double lat, double lon)> ParseLine(JArray arr)
        {
            var list = new List<(double lat, double lon)>(arr.Count);
            foreach (var pt in arr)
            {
                if (pt is not JArray a || a.Count < 2) continue;
                if ((a[0].Type != JTokenType.Float && a[0].Type != JTokenType.Integer) ||
                    (a[1].Type != JTokenType.Float && a[1].Type != JTokenType.Integer)) continue;
                double lon = a[0].ToObject<double>();
                double lat = a[1].ToObject<double>();
                list.Add((lat, lon));
            }
            return list;
        }
    }
}
