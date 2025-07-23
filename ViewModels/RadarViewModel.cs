using OpenTK.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Collections.Generic;
using System.Windows.Input;
using vFalcon.Helpers;
using vFalcon.Services;
using vFalcon.Models;
using vFalcon.Rendering;

namespace vFalcon.ViewModels
{
    public class RadarViewModel : ViewModelBase
    {
        private Profile profile;
        private MapRenderer _renderer;
        private MapState _mapState;
        private VideoMap _videoMap;
        private PilotRenderer _pilotRenderer;
        private PilotService _pilotService;
        private VatsimDataService _vatsimDataService;
        private ArtccBox? _artccBox;

        private readonly List<Pilot> _pilots; // ideally use MapDataService here
        private SKPoint? _lastMousePosition = null;
        private bool _isFirstRender = true;

        public Action? InvalidateCanvas;
        public Action<bool>? SetCursorVisibility { get; set; }
        public Action? CaptureMouse { get; set; }
        public Action? ReleaseMouse { get; set; }
        public bool ZoomOnMouse { get; set; } = false;

        public ICommand PaintSurfaceCommand { get; }
        public ICommand MouseDownCommand { get; }
        public ICommand MouseMoveCommand { get; }
        public ICommand MouseUpCommand { get; }
        public ICommand MouseWheelCommand { get; }

        public RadarViewModel(Profile profile)
        {
            this.profile = profile;

            _mapState = new MapState();
            _videoMap = new VideoMap();
            _pilotRenderer = new PilotRenderer();
            _renderer = new MapRenderer(_mapState, _videoMap, _pilotRenderer);
            _pilots = new List<Pilot>();

            _artccBox = LoadArtccBox();

            _pilotService = new PilotService();
            _vatsimDataService = new VatsimDataService(_pilotService, profile, _artccBox, () => InvalidateCanvas?.Invoke());
            _vatsimDataService.Start();

            MouseDownCommand = new RelayCommand(OnMouseDown);
            MouseMoveCommand = new RelayCommand(OnMouseMove);
            MouseUpCommand = new RelayCommand(OnMouseUp);
            MouseWheelCommand = new RelayCommand(OnMouseWheel);
            PaintSurfaceCommand = new RelayCommand(OnPaintSurface);
        }

        public void UpdateVatsimDataService()
        {
            _vatsimDataService.Refresh();
            InvalidateCanvas?.Invoke();
        }

        public ArtccBox? LoadArtccBox()
        {
            _artccBox = ArtccBox.Load(profile.ArtccId);
            Coordinate center = _artccBox.GetCenter();
            _mapState.CenterLat = center.Latitude;
            _mapState.CenterLon = center.Longitude;
            _mapState.CurrentLat = center.Latitude;
            _mapState.CurrentLon = center.Longitude;
            return _artccBox;
        }

        public void LoadVideoMap(string file, string color)
        {
            _videoMap.Load(file, color);
        }

        public void UnloadVideoMap(string file)
        {
            _videoMap.Unload(file);
        }

        private void OnMouseDown(object parameter)
        {
            if (parameter is SKMouseEventArgs e && e.Button == "Right")
            {
                CaptureMouse?.Invoke();
                _lastMousePosition = e.Location;
                InvalidateCanvas?.Invoke();
            }
        }

        private void OnMouseMove(object parameter)
        {
            if (parameter is SKMouseEventArgs e && _lastMousePosition.HasValue && e.Button == "Right")
            {
                SetCursorVisibility?.Invoke(false); // hide
                var delta = e.Location - _lastMousePosition.Value;
                _mapState.PanOffset = new SKPoint(
                    _mapState.PanOffset.X + delta.X,
                    _mapState.PanOffset.Y + delta.Y
                );
                _lastMousePosition = e.Location;
                InvalidateCanvas?.Invoke();
            }
        }

        private void OnMouseUp(object parameter)
        {
            if (parameter is SKMouseEventArgs e && e.Button == "Right")
            {
                SetCursorVisibility?.Invoke(true); // hide
                ReleaseMouse?.Invoke();
                _lastMousePosition = null;
                InvalidateCanvas?.Invoke();
            }
        }

        private void OnMouseWheel(object parameter)
        {
            if (parameter is SKMouseWheelEventArgs e)
            {
                float zoomFactor = e.Delta > 0 ? 1.1f : 0.9f;
                SKPoint referencePoint = ZoomOnMouse ? e.Location : new SKPoint(_mapState.Width / 2f, _mapState.Height / 2f);

                var before = new SKPoint(
                    (referencePoint.X - _mapState.PanOffset.X - _mapState.Width / 2f) / (float)_mapState.Scale,
                    (referencePoint.Y - _mapState.PanOffset.Y - _mapState.Height / 2f) / (float)_mapState.Scale
                );

                _mapState.Scale *= zoomFactor;

                var after = new SKPoint(
                    (referencePoint.X - _mapState.PanOffset.X - _mapState.Width / 2f) / (float)_mapState.Scale,
                    (referencePoint.Y - _mapState.PanOffset.Y - _mapState.Height / 2f) / (float)_mapState.Scale
                );

                var diff = after - before;

                _mapState.PanOffset = new SKPoint(
                    _mapState.PanOffset.X + diff.X * (float)_mapState.Scale,
                    _mapState.PanOffset.Y + diff.Y * (float)_mapState.Scale
                );

                InvalidateCanvas?.Invoke();
            }
        }

        private void OnPaintSurface(object parameter)
        {
            if (parameter is SKPaintSurfaceEventArgs e)
            {
                _mapState.Width = e.Info.Width;
                _mapState.Height = e.Info.Height;

                if (_isFirstRender)
                {
                    var panOffset = _mapState.PanOffset;
                    VideoMap.CenterAtCoordinates(
                        _mapState.Width,
                        _mapState.Height,
                        _mapState.Scale,
                        ref panOffset,
                        _mapState.CenterLat,
                        _mapState.CenterLon
                    );
                    _mapState.PanOffset = panOffset;
                    _isFirstRender = false;
                }

                var canvas = e.Surface.Canvas;
                canvas.Clear();
                _renderer.Render(canvas, _mapState, _pilotService.Pilots.Values);
            }
        }
    }
}
