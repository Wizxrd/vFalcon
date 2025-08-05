using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Windows.Input;
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

        private List<double> ZoomLevels = BuildZoomLevels();
        private List<double> ScaleMap;

        private int zoomIndex; //default index to match Range 600

        private bool isFirstRender = true;

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

        VatsimDataService vatsimDataService;
        PilotService pilotService;
        private PilotRenderer pilotRenderer;
        public RadarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;

            MouseDownCommand = new RelayCommand(OnMouseDown);
            MouseMoveCommand = new RelayCommand(OnMouseMove);
            MouseUpCommand = new RelayCommand(OnMouseUp);
            MouseWheelCommand = new RelayCommand(OnMouseWheel);
            PaintSurfaceCommand = new RelayCommand(OnPaintCommand);

            InitializeCenterCoordinates();

            pilotRenderer = new PilotRenderer();
            pilotService = new PilotService(eramViewModel.artcc);
            vatsimDataService = new VatsimDataService(pilotService, eramViewModel.profile, () => InvalidateCanvas?.Invoke());
            vatsimDataService.Start();

        }

        // ========================================================
        //                      PUBLIC METHODS
        // ========================================================
        public void Redraw() => InvalidateCanvas?.Invoke();
        public string GetCurrentZoomString() { return ZoomLevels[zoomIndex].ToString("0.##"); }

        // ========================================================
        //                      INITIALIZATION
        // ========================================================

        private void InitializeCenterCoordinates()
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

        private static List<double> BuildZoomLevels()
        {
            var levels = new List<double>();

            for (double z = 0.25; z <= 2; z += 0.25) levels.Add(z); // fine increments for close-in
            for (double z = 3; z <= 10; z += 1) levels.Add(z);      // 3 → 10 in steps of 1
            for (double z = 20; z <= 1300; z += 10) levels.Add(z);  // 20 → 1300 in steps of 10

            return levels; // ascending order
        }

        private List<double> BuildScaleMap(int screenHeight, SKPoint panOffset)
        {
            var list = new List<double>();

            foreach (var nm in ZoomLevels)
            {
                double testScale = 1.0;

                var top = ScreenMap.ScreenToCoordinate(new System.Drawing.Size(1000, screenHeight), testScale, panOffset, new SKPoint(500, 0));
                var bottom = ScreenMap.ScreenToCoordinate(new System.Drawing.Size(1000, screenHeight), testScale, panOffset, new SKPoint(500, screenHeight));

                double currentDistance = ScreenMap.DistanceInNM(top.X, top.Y, bottom.X, bottom.Y);
                double adjustedScale = testScale * (currentDistance / nm);
                list.Add(adjustedScale);
            }
            return list;
        }

        // ========================================================
        //                      MOUSE EVENTS
        // ========================================================
        private void OnMouseDown(object parameter)
        {
            if (parameter is SKMouseEventArgs e && e.Button == "Right")
            {
                _lastMousePosition = e.Location;
                Redraw();
            }
        }

        private void OnMouseMove(object parameter)
        {
            if (parameter is SKMouseEventArgs e && _lastMousePosition.HasValue && e.Button == "Right")
            {
                var delta = e.Location - _lastMousePosition.Value;
                mapState.PanOffset = new SKPoint(mapState.PanOffset.X + delta.X, mapState.PanOffset.Y + delta.Y);
                _lastMousePosition = e.Location;
                Redraw();
            }
        }

        private void OnMouseUp(object parameter)
        {
            if (parameter is SKMouseEventArgs e && e.Button == "Right")
            {
                _lastMousePosition = null;
                Redraw();
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
                mapState.PanOffset = new SKPoint(mapState.PanOffset.X + diff.X * (float)mapState.Scale, mapState.PanOffset.Y + diff.Y * (float)mapState.Scale);

                mapState.ZoomIndex = (int)ZoomLevels[zoomIndex];
                ZoomLevelChanged?.Invoke(ZoomLevels[zoomIndex].ToString("0.##"));
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
                ScaleMap = BuildScaleMap(mapState.Height, mapState.PanOffset);
                zoomIndex = ZoomLevels.Count - 1;
                mapState.Scale = ScaleMap[zoomIndex];
                VideoMap.CenterAtCoordinates(mapState.Width, mapState.Height, mapState.Scale, ref pOffset, mapState.CenterLat, mapState.CenterLon);
                mapState.PanOffset = pOffset;
                isFirstRender = false;
            }
            var activeFeatures = CollectActiveFeatures();
            videoMap.Render(canvas, new System.Drawing.Size(mapState.Width, mapState.Height), mapState.Scale, mapState.PanOffset, activeFeatures);

            var size = new Size(mapState.Width, mapState.Height);
            foreach (var pilot in pilotService.Pilots.Values)
            {
                pilotRenderer.Render(pilot, canvas, size, mapState.Scale, mapState.PanOffset);
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
