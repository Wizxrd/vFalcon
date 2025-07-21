using SkiaSharp;

namespace vFalcon.Models
{
    public class MapState
    {
        public double Scale { get; set; } = 0.0112332;
        public SKPoint PanOffset { get; set; } = SKPoint.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public double CenterLat { get; set; } = 40.93177398650218;
        public double CenterLon { get; set; } = -80.82024227841458;
        public double CurrentLat { get; set; } = 40.93177398650218;
        public double CurrentLon { get; set; } = -80.82024227841458;

        public int ZoomIndex { get; set; } = 25;
    }
}