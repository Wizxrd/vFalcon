using OpenTK.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Collections.Generic;
using System.Windows.Input;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Rendering;

namespace vFalcon.ViewModels
{
    public class RadarViewModel : ViewModelBase
    {

        private readonly MapRenderer _renderer;
        private readonly MapState _state;
        private readonly VideoMap _videoMap;
        private readonly List<Pilot> _pilots; // ideally use MapDataService here
        private SKPoint? _lastMousePosition = null;
        private bool _isFirstRender = true;

        public Action? InvalidateCanvas;
        public Action<bool>? SetCursorVisibility { get; set; }

        public ICommand PaintSurfaceCommand { get; }
        public ICommand MouseDownCommand { get; }
        public ICommand MouseMoveCommand { get; }
        public ICommand MouseUpCommand { get; }
        public ICommand MouseWheelCommand { get; }


        public RadarViewModel()
        {
            _state = new MapState(); // later inject size/center/etc.
            _videoMap = new VideoMap();
            var pilotRenderer = new PilotRenderer();
            _renderer = new MapRenderer(_videoMap, pilotRenderer);
            _pilots = new List<Pilot>();

            MouseDownCommand = new RelayCommand(OnMouseDown);
            MouseMoveCommand = new RelayCommand(OnMouseMove);
            MouseUpCommand = new RelayCommand(OnMouseUp);
            MouseWheelCommand = new RelayCommand(OnMouseWheel);
            PaintSurfaceCommand = new RelayCommand(OnPaintSurface);
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
                SetCursorVisibility?.Invoke(false); // hide
                _lastMousePosition = e.Location;
                InvalidateCanvas?.Invoke();
            }
        }

        private void OnMouseMove(object parameter)
        {
            if (parameter is SKMouseEventArgs e && _lastMousePosition.HasValue && e.Button == "Right")
            {
                var delta = e.Location - _lastMousePosition.Value;
                _state.PanOffset = new SKPoint(
                    _state.PanOffset.X + delta.X,
                    _state.PanOffset.Y + delta.Y
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
                _lastMousePosition = null;
                InvalidateCanvas?.Invoke();
            }
        }

        private void OnMouseWheel(object parameter)
        {
            if (parameter is SKMouseWheelEventArgs e)
            {
                float zoomFactor = e.Delta > 0 ? 1.1f : 0.9f;

                var mouse = e.Location;

                var before = new SKPoint(
                    (mouse.X - _state.PanOffset.X - _state.Width / 2f) / (float)_state.Scale,
                    (mouse.Y - _state.PanOffset.Y - _state.Height / 2f) / (float)_state.Scale
                );

                _state.Scale *= zoomFactor;

                var after = new SKPoint(
                    (mouse.X - _state.PanOffset.X - _state.Width / 2f) / (float)_state.Scale,
                    (mouse.Y - _state.PanOffset.Y - _state.Height / 2f) / (float)_state.Scale
                );

                var diff = after - before;

                _state.PanOffset = new SKPoint(
                    _state.PanOffset.X + diff.X * (float)_state.Scale,
                    _state.PanOffset.Y + diff.Y * (float)_state.Scale
                );
            }
            InvalidateCanvas?.Invoke();
        }

        private void OnPaintSurface(object parameter)
        {
            if (parameter is SKPaintSurfaceEventArgs e)
            {
                _state.Width = e.Info.Width;
                _state.Height = e.Info.Height;

                if (_isFirstRender)
                {
                    var panOffset = _state.PanOffset;
                    VideoMap.CenterAtCoordinates(
                        _state.Width,
                        _state.Height,
                        _state.Scale,
                        ref panOffset,
                        _state.CenterLat,
                        _state.CenterLon
                    );
                    _state.PanOffset = panOffset;
                    _isFirstRender = false;
                }

                var canvas = e.Surface.Canvas;
                canvas.Clear();
                _renderer.Render(canvas, _state, _pilots);
            }
        }
    }
}
