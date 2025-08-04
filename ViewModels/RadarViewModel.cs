using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Renderers;

namespace vFalcon.ViewModels
{
    public class RadarViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;
        private readonly VideoMap videoMap;

        private SKPoint PanOffset = new SKPoint();
        private double Scale = 0.306;
        private int Width;
        private int Height;

        private static readonly List<double> ZoomLevels = BuildZoomLevels();
        private static readonly List<double> ScaleMap = BuildScaleMap();
        private int _zoomIndex = 144;

        private double CenterLat = 41.223661784766726;
        private double CenterLon = -80.9481329717042;
        private bool isFirstRender = true;
        private SKPoint? _lastMousePosition = null;

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public Action? InvalidateCanvas;
        public Action<string>? ZoomLevelChanged;

        public int ZoomIndex { get; set; } = 144;
        public bool ZoomOnMouse { get; set; } = false;

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
            videoMap = new VideoMap();

            MouseDownCommand = new RelayCommand(OnMouseDown);
            MouseMoveCommand = new RelayCommand(OnMouseMove);
            MouseUpCommand = new RelayCommand(OnMouseUp);
            MouseWheelCommand = new RelayCommand(OnMouseWheel);
            PaintSurfaceCommand = new RelayCommand(OnPaintCommand);

            InitializeCenterCoordinates();
        }

        // ========================================================
        //                      PUBLIC METHODS
        // ========================================================
        public void Redraw() => InvalidateCanvas?.Invoke();
        public string GetCurrentZoomString() { return ZoomLevels[_zoomIndex].ToString("0.##"); }

        // ========================================================
        //                      INITIALIZATION
        // ========================================================
        private void InitializeCenterCoordinates()
        {
            JToken centerToken = eramViewModel.profile.DisplayWindowSettings?[0]?["DisplaySettings"]?[0]?["Center"];
            if (centerToken is JObject centerObj)
            {
                CenterLat = (double)centerObj["Lat"];
                CenterLon = (double)centerObj["Lon"];
            }
            else
            {
                CenterLat = (double)eramViewModel.artcc.visibilityCenters[0]["lat"];
                CenterLon = (double)eramViewModel.artcc.visibilityCenters[0]["lon"];
            }
        }

        private static List<double> BuildZoomLevels()
        {
            var levels = new List<double>();
            for (double z = 1300; z >= 10; z -= 10) levels.Add(z);
            for (double z = 9; z >= 2; z -= 1) levels.Add(z);
            for (double z = 1.75; z >= 0.25; z -= 0.25) levels.Add(z);
            levels.Reverse();
            return levels;
        }

        private static List<double> BuildScaleMap()
        {
            double minScale = 0.306;
            double maxScale = 139.0;
            int count = ZoomLevels.Count;

            var list = new List<double>(count);
            for (int i = 0; i < count; i++)
            {
                double t = (double)i / (count - 1);
                double logT = Math.Pow(t, 0.01);
                double scale = minScale + (1.0 - logT) * (maxScale - minScale);
                list.Add(scale);
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
                PanOffset = new SKPoint(PanOffset.X + delta.X, PanOffset.Y + delta.Y);
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
                int newIndex = Math.Clamp(_zoomIndex + delta, 0, ZoomLevels.Count - 1);
                if (newIndex == _zoomIndex) return;

                SKPoint referencePoint = ZoomOnMouse ? e.Location : new SKPoint(Width / 2f, Height / 2f);

                var before = new SKPoint(
                    (referencePoint.X - PanOffset.X - Width / 2f) / (float)Scale,
                    (referencePoint.Y - PanOffset.Y - Height / 2f) / (float)Scale
                );

                _zoomIndex = newIndex;
                Scale = ScaleMap[_zoomIndex];

                var after = new SKPoint(
                    (referencePoint.X - PanOffset.X - Width / 2f) / (float)Scale,
                    (referencePoint.Y - PanOffset.Y - Height / 2f) / (float)Scale
                );

                var diff = after - before;
                PanOffset = new SKPoint(PanOffset.X + diff.X * (float)Scale, PanOffset.Y + diff.Y * (float)Scale);

                ZoomIndex = (int)ZoomLevels[_zoomIndex];
                ZoomLevelChanged?.Invoke(ZoomLevels[_zoomIndex].ToString("0.##"));
                Redraw();
            }
        }

        // ========================================================
        //                      PAINTING
        // ========================================================
        private void OnPaintCommand(object parameter)
        {
            if (parameter is not SKPaintSurfaceEventArgs e) return;

            Width = e.Info.Width;
            Height = e.Info.Height;

            var canvas = e.Surface.Canvas;
            canvas.Clear();

            if (isFirstRender)
            {
                var pOffset = PanOffset;
                VideoMap.CenterAtCoordinates(Width, Height, Scale, ref pOffset, CenterLat, CenterLon);
                PanOffset = pOffset;
                isFirstRender = false;
            }

            var activeFeatures = CollectActiveFeatures();
            videoMap.Render(canvas, new System.Drawing.Size(Width, Height), Scale, PanOffset, activeFeatures);
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
