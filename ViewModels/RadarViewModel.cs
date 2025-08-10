using AdonisUI.Converters;
using NAudio.Gui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Renderers;
using vFalcon.Rendering;
using vFalcon.Services.Service;
using static System.Windows.Forms.AxHost;

namespace vFalcon.ViewModels
{
    public class RadarViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;
        private readonly VideoMap videoMap = new VideoMap();
        public MapState mapState = new MapState();
        private VatsimDataService vatsimDataService;
        public PilotService pilotService;
        private PilotRenderer pilotRenderer;

        public List<double> ZoomLevels = BuildZoomLevels();
        private List<double> ScaleMap;

        public int zoomIndex;

        private bool isFirstRender = true;
        public bool PilotsRendering = false;

        private SKPoint? _lastMousePosition = null;

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public Action? InvalidateCanvas;
        public Action<string>? ZoomLevelChanged;

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand PaintSurfaceCommand { get; }
        public ICommand MouseDownCommand { get; }
        public ICommand MouseMoveCommand { get; }
        public ICommand MouseUpCommand { get; }
        public ICommand MouseWheelCommand { get; }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================

        public RadarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;

            MouseDownCommand = new RelayCommand(OnMouseDown);
            MouseMoveCommand = new RelayCommand(OnMouseMove);
            MouseUpCommand = new RelayCommand(OnMouseUp);
            MouseWheelCommand = new RelayCommand(OnMouseWheel);
            PaintSurfaceCommand = new RelayCommand(OnPaintCommand);

            InitializeRange();
            InitializeCenterCoordinates();
        }

        // ========================================================
        //                      PUBLIC METHODS
        // ========================================================

        public void InitializeVatsimServices()
        {
            if (pilotRenderer == null) //nothing else *should* be null so only check pilotRenderer 
            {
                pilotRenderer = new PilotRenderer(eramViewModel);
                pilotService = new PilotService(eramViewModel.artcc);
                vatsimDataService = new VatsimDataService(pilotService, eramViewModel.profile, () => InvalidateCanvas?.Invoke());
                vatsimDataService.Start();
                PilotsRendering = true;
            }
        }

        public void SetZoomOnMouse(bool zoomOnMouse)
        {
            mapState.ZoomOnMouse = zoomOnMouse;
        }

        public void Redraw()
        {
            InvalidateCanvas?.Invoke();
        }

        public string GetCurrentZoomString() { return ZoomLevels[zoomIndex].ToString("0.##"); }

        public void UpdateVatsimDataService()
        {
            vatsimDataService.Refresh();
            Redraw();
        }

        public void EramSizeChanged(SizeChangedEventArgs e)
        {
            mapState.Width = (int)e.NewSize.Width;
            mapState.Height = (int)e.NewSize.Height;

            var p = mapState.PanOffset;
            VideoMap.CenterAtCoordinates(mapState.Width, mapState.Height, mapState.Scale, ref p, mapState.CenterLat, mapState.CenterLon);
            mapState.PanOffset = p;

            ScaleMap = BuildScaleMap(mapState.Width, mapState.Height, mapState.CenterLat, mapState.CenterLon);

            mapState.Scale = ScaleMap[zoomIndex];

            p = mapState.PanOffset;
            VideoMap.CenterAtCoordinates(mapState.Width, mapState.Height, mapState.Scale, ref p, mapState.CenterLat, mapState.CenterLon);
            mapState.PanOffset = p;

            Redraw();
        }

        public Pilot? TargetClickedOn(SKPoint mousePoint)
        {
            if (eramViewModel.RadarViewModel.pilotService == null) return null;
            const double clickRadius = 10f; // click radius
            System.Drawing.Size size = new System.Drawing.Size(mapState.Width, mapState.Height);
            var pilots = eramViewModel.RadarViewModel.pilotService.Pilots.Values.ToList() ?? new List<Pilot>();
            foreach (Pilot pilot in pilots)
            {
                SKPoint pilotScreenPos = ScreenMap.CoordinateToScreen(size.Width, size.Height, mapState.Scale, mapState.PanOffset, pilot.Latitude, pilot.Longitude);
                pilotScreenPos.Y += 30;
                float dx = pilotScreenPos.X - mousePoint.X;
                float dy = pilotScreenPos.Y - mousePoint.Y;
                double distanceSquared = Math.Sqrt(dx * dx + dy * dy);

                if (distanceSquared <= clickRadius)
                {
                    return pilot;
                }
            }
            return null;
        }

        public Pilot? EramLeftMouseDown(SKPoint mousePoint)
        {
            Pilot? clickedTarget = TargetClickedOn(mousePoint);
            if (clickedTarget != null)
            {
                return clickedTarget;
            }
            return null;
        }

        public void EramMiddleMouseDown(object sender, MouseEventArgs e)
        {
            System.Windows.Point pos = e.GetPosition((IInputElement)sender);
            SKPoint mousePoint = new SKPoint((float)pos.X, (float)pos.Y);
            Pilot? targetClicked = TargetClickedOn(mousePoint);
            if (targetClicked != null)
            {
                targetClicked.ForcedFullDataBlock = !targetClicked.ForcedFullDataBlock;
                targetClicked.FullDataBlock = !targetClicked.FullDataBlock;
                Redraw();
            }
        }

        // ========================================================
        //                      INITIALIZATION
        // ========================================================

        private void InitializeRange()
        {
            int l = (int)eramViewModel.profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Range"];
            Logger.Debug("RANGE", l.ToString());
            int i = 0;
            foreach (int level in ZoomLevels)
            {
                Logger.Debug("LEVLE", level.ToString());
                if (level == l)
                {
                    Logger.Debug("MATCH", i.ToString());
                    zoomIndex = i;
                    break;
                }
                i++;
            }
        }

        private void InitializeCenterCoordinates()
        {
            try
            {
                JToken centerToken = eramViewModel.profile.DisplayWindowSettings?[0]?["DisplaySettings"]?[0]?["Center"];
                if (centerToken is JObject centerObj)
                {
                    mapState.CenterLat = (double)centerObj["Lat"];
                    mapState.CenterLon = (double)centerObj["Lon"];
                }
                else
                {
                    mapState.CenterLat = (double)eramViewModel.artcc.visibilityCenters[0]["lat"];
                    mapState.CenterLon = (double)eramViewModel.artcc.visibilityCenters[0]["lon"];
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("RadarViewModel.InitializeCenterCoordinates", ex.ToString());
            }
        }

        private static List<double> BuildZoomLevels()
        {
            var levels = new List<double>();

            for (double z = 0.25; z <= 2; z += 0.25) levels.Add(z); // fine increments for close-in
            for (double z = 3; z <= 10; z += 1) levels.Add(z);      // 3 → 10 in steps of 1
            for (double z = 20; z <= 1300; z += 10) levels.Add(z);  // 20 → 1300 in steps of 10

            return levels; // ascending order
        }
        private List<double> BuildScaleMap(int screenWidth, int screenHeight, double centerLat, double centerLon)
        {
            var list = new List<double>();
            var screenSize = new System.Drawing.Size(screenWidth, screenHeight);

            // Use a fixed test scale; 1.0 is fine.
            const double testScale = 0.025;

            // Compute a panOffset that centers centerLat/centerLon at this test scale.
            var panForTest = new SKPoint(0, 0);
            VideoMap.CenterAtCoordinates(screenWidth, screenHeight, testScale, ref panForTest, centerLat, centerLon);

            // We’re defining zoom as NM across HEIGHT (since your current code measures top→bottom).
            var start = new SKPoint(screenWidth / 2f, 0);
            var end = new SKPoint(screenWidth / 2f, screenHeight);

            foreach (var nm in ZoomLevels)
            {
                var c1 = ScreenMap.ScreenToCoordinate(screenSize, testScale, panForTest, start);
                var c2 = ScreenMap.ScreenToCoordinate(screenSize, testScale, panForTest, end);

                double heightNm = ScreenMap.DistanceInNM(c1.X, c1.Y, c2.X, c2.Y);

                // Scale that would make the screen height equal `nm`
                double adjustedScale = testScale * (heightNm / nm);
                list.Add(adjustedScale);
            }

            return list;
        }
        // ========================================================
        //                      MOUSE EVENTS
        // ========================================================
        private bool isPanning = false; // Flag to track whether panning is in progress

        private readonly object panLock = new object();
        private void OnMouseDown(object parameter)
        {
            if (parameter is SKMouseEventArgs e && e.Button == "Right")
            {
                if (_lastMousePosition == e.Location) return;
                _lastMousePosition = e.Location;
                isPanning = true;
                // Mouse is captured in the Behavior. Nothing to do here.
            }
        }

        private void OnMouseMove(object parameter)
        {
            if (!isPanning || parameter is not SKMouseEventArgs e || !_lastMousePosition.HasValue)
                return;

            // Behavior sends Button="Right" while pressed; if yours doesn't, remove this check.
            if (e.Button != "Right")
                return;

            var delta = e.Location - _lastMousePosition.Value;

            mapState.PanOffset = new SKPoint(
                mapState.PanOffset.X + delta.X,
                mapState.PanOffset.Y + delta.Y
            );

            _lastMousePosition = e.Location;
            eramViewModel.eramView.HideCursor();
            // We're already on UI thread via Behavior event
            Redraw();
        }

        private void OnMouseUp(object parameter)
        {
            if (parameter is SKMouseEventArgs e && e.Button == "Right")
            {
                var newCenter = ScreenMap.ScreenToCoordinate(
                    new System.Drawing.Size(mapState.Width, mapState.Height),
                    mapState.Scale,
                    mapState.PanOffset,
                    new SKPoint(mapState.Width / 2f, mapState.Height / 2f)
                );

                mapState.CenterLat = newCenter.X;
                mapState.CenterLon = newCenter.Y;

                _lastMousePosition = null;
                isPanning = false;
                eramViewModel.eramView.ShowCursor();
                // Capture is released in the Behavior. Nothing to do here.
            }
        }

        private void OnMouseWheel(object parameter)
        {
            if (parameter is SKMouseWheelEventArgs e)
            {
                int delta = e.Delta > 0 ? -1 : 1;
                int newIndex = Math.Clamp(zoomIndex + delta, 0, ZoomLevels.Count - 1);
                if (newIndex == zoomIndex) return;

                SKPoint referencePoint = mapState.ZoomOnMouse ? e.Location : new SKPoint(mapState.Width / 2f, mapState.Height / 2f);

                var before = new SKPoint(
                    (referencePoint.X - mapState.PanOffset.X - mapState.Width / 2f) / (float)mapState.Scale,
                    (referencePoint.Y - mapState.PanOffset.Y - mapState.Height / 2f) / (float)mapState.Scale
                );

                zoomIndex = newIndex;

                mapState.Scale = ScaleMap[zoomIndex];

                var after = new SKPoint(
                    (referencePoint.X - mapState.PanOffset.X - mapState.Width / 2f) / (float)mapState.Scale,
                    (referencePoint.Y - mapState.PanOffset.Y - mapState.Height / 2f) / (float)mapState.Scale
                );

                var diff = after - before;
                mapState.PanOffset = new SKPoint(
                    mapState.PanOffset.X + diff.X * (float)mapState.Scale,
                    mapState.PanOffset.Y + diff.Y * (float)mapState.Scale
                );

                mapState.ZoomIndex = (int)ZoomLevels[zoomIndex];
                ZoomLevelChanged?.Invoke(ZoomLevels[zoomIndex].ToString("0.##"));

                if (!mapState.ZoomOnMouse)
                {
                    var pOffset = mapState.PanOffset;
                    VideoMap.CenterAtCoordinates(mapState.Width, mapState.Height, mapState.Scale, ref pOffset, mapState.CenterLat, mapState.CenterLon);
                    mapState.PanOffset = pOffset;
                }
                else
                {
                    var newCenter = ScreenMap.ScreenToCoordinate(
                        new System.Drawing.Size(mapState.Width, mapState.Height),
                        mapState.Scale,
                        mapState.PanOffset,
                        new SKPoint(mapState.Width / 2f, mapState.Height / 2f)
                    );

                    mapState.CenterLat = newCenter.X;
                    mapState.CenterLon = newCenter.Y;
                    var pOffset = mapState.PanOffset;
                    mapState.PanOffset = pOffset;
                }

                Redraw();
            }
        }


        // ========================================================
        //                      PAINTING
        // ========================================================
        private void OnPaintCommand(object parameter)
        {
            if (parameter is not SKPaintSurfaceEventArgs e) return;

            mapState.Width = e.Info.Width;
            mapState.Height = e.Info.Height;

            var canvas = e.Surface.Canvas;
            canvas.Clear();
            if (isFirstRender)
            {
                var pOffset = mapState.PanOffset;
                mapState.Scale = ScaleMap[zoomIndex];
                mapState.ZoomIndex = (int)ZoomLevels[zoomIndex];
                VideoMap.CenterAtCoordinates(mapState.Width, mapState.Height, mapState.Scale, ref pOffset, mapState.CenterLat, mapState.CenterLon);
                mapState.PanOffset = pOffset;
                isFirstRender = false;
            }

            var activeFeatures = CollectActiveFeatures();
            videoMap.Render(eramViewModel, canvas, new System.Drawing.Size(mapState.Width, mapState.Height), mapState.Scale, mapState.PanOffset, activeFeatures);

            if (pilotService != null)
            {
                System.Drawing.Size size = new System.Drawing.Size(mapState.Width, mapState.Height);
                var pilots = pilotService.Pilots.Values.ToList();
                foreach (var pilot in pilots)
                {
                    if (!pilot.JRingEnabled) continue;
                    pilotRenderer.RenderJRing(canvas, size, mapState.Scale, mapState.PanOffset, pilot);
                }
                foreach (var pilot in pilots)
                {
                    pilotRenderer.RenderHistory(canvas, size, mapState.Scale, mapState.PanOffset, pilot);
                }
                foreach (var pilot in pilots)
                {
                    pilotRenderer.RenderPilot(pilot, canvas, size, mapState.Scale, mapState.PanOffset);
                }
            }
        }

        // ========================================================
        //                      FEATURE FILTERING
        // ========================================================
        private List<ProcessedFeature> CollectActiveFeatures()
        {
            var activeFeatures = new List<ProcessedFeature>();
            foreach (var kvp in eramViewModel.FacilityFeatures)
            {
                if (!eramViewModel.ActiveFilters.Contains(kvp.Key)) continue;

                var filtered = kvp.Value.Where(f =>
                {
                    if (f.AppliedAttributes.TryGetValue("tdmOnly", out var tdmVal))
                    {
                        bool isTdmOnly = Convert.ToBoolean(tdmVal);
                        if (isTdmOnly && !eramViewModel.TdmActive)
                            return false;
                    }
                    return true;
                });

                activeFeatures.AddRange(filtered);
            }

            return activeFeatures;
        }
    }
}
