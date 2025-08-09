using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Models
{
    public class MapState
    {
        public double Scale { get; set; }
        public SKPoint PanOffset { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double CenterLat { get; set; }
        public double CenterLon { get; set; }
        public int ZoomIndex { get; set; }
        public bool ZoomOnMouse { get; set; }
    }
}
