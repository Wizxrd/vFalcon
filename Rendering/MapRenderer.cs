using SkiaSharp;
using System.Collections.Generic;
using System.Drawing;
using vFalcon.Models;

namespace vFalcon.Rendering
{
    public class MapRenderer
    {
        private readonly VideoMap _videoMap;
        private readonly PilotRenderer _pilotRenderer;

        public MapRenderer(VideoMap videoMap, PilotRenderer pilotRenderer)
        {
            _videoMap = videoMap;
            _pilotRenderer = pilotRenderer;
        }

        public void Render(SKCanvas canvas, MapState state, IEnumerable<Pilot> pilots)
        {
            var canvasSize = new Size(state.Width, state.Height);
            _videoMap.Render(canvas, canvasSize, state.Scale, state.PanOffset);

            foreach (var pilot in pilots)
            {
                _pilotRenderer.Render(pilot, canvas, canvasSize, state.Scale, state.PanOffset);
            }
        }
    }
}