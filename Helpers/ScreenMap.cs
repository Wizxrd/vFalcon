using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Helpers
{
    public class ScreenMap
    {
        private static double EarthRadiusKM = 6371.0;
        private static double RadPerDeg = Math.PI / 180.0;

        public static SKPoint CoordinateToScreen(int width, int height, double scale, SKPoint panOffset, double lat, double lon, double centerLat = 0, double centerLon = 0)
        {
            double x = EarthRadiusKM * (lon - centerLon) * Math.Cos(centerLat * RadPerDeg);
            double y = EarthRadiusKM * (lat - centerLat);
            int screenX = (int)(x * scale + width / 2 + panOffset.X);
            int screenY = (int)(-y * scale + height / 2 + panOffset.Y);
            return new SKPoint(screenX, screenY);
        }

        public static SKPoint ScreenToCoordinate(Size size, double scale, SKPoint panOffset, SKPoint point, double centerLat = 0, double centerLon = 0)
        {
            int screenWidth = size.Width;
            int screenHeight = size.Height;
            double x = (point.X - screenWidth / 2 - panOffset.X) / scale;
            double y = -(point.Y - screenHeight / 2 - panOffset.Y) / scale;
            double lat = centerLat + (y / EarthRadiusKM);
            double lon = centerLon + (x / (EarthRadiusKM * Math.Cos(centerLat * RadPerDeg)));
            return new SKPoint((float)lat, (float)lon);
        }
    }
}
