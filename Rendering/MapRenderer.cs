using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services;

namespace vFalcon.Rendering
{
    public class MapRenderer
    {
        private readonly Profile _profile;
        private readonly MapState _mapState;
        private readonly VideoMap _videoMap;
        private readonly PilotRenderer _pilotRenderer;

        public MapRenderer(MapState _state, VideoMap videoMap, PilotRenderer pilotRenderer)
        {
            _mapState = _state;
            _videoMap = videoMap;
            _pilotRenderer = pilotRenderer;
        }

        public void Render(SKCanvas canvas, MapState state, IEnumerable<Pilot> pilots)
        {
            try
            {
                var canvasSize = new Size(state.Width, state.Height);
                _videoMap.Render(canvas, canvasSize, state.Scale, state.PanOffset);

                foreach (var pilot in pilots)
                {
                    _pilotRenderer.Render(pilot, canvas, canvasSize, state.Scale, state.PanOffset);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("MapRenderer.Render", ex.ToString());
            }
        }
    }
}