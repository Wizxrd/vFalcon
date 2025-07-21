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
        public ICommand PaintSurfaceCommand { get; }

        private readonly MapRenderer _renderer;
        private readonly MapState _state;
        private readonly List<Pilot> _pilots; // ideally use MapDataService here

        private bool _isFirstRender = true;

        public RadarViewModel()
        {
            _state = new MapState(); // later inject size/center/etc.
            var videoMap = new VideoMap();
            var pilotRenderer = new PilotRenderer();
            _renderer = new MapRenderer(videoMap, pilotRenderer);
            _pilots = new List<Pilot>();
            string filePath = Loader.LoadFile("VideoMaps", "HIGH_SECTORS.geojson");
            videoMap.Load(filePath, "#00ff00"); // choose color as needed
            var panOffset = _state.PanOffset;
            //VideoMap.CenterAtCoordinates(_state.Width, _state.Height, _state.Scale, ref panOffset, _state.CenterLat, _state.CenterLon);
            PaintSurfaceCommand = new RelayCommand(OnPaintSurface);
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
                _renderer.Render(canvas, _state, _pilots);
            }
        }
    }
}
