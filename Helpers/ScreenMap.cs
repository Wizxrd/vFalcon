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
            double x = EarthRadiusKM * (lon - centerLon) * RadPerDeg;

            double latRad = lat * RadPerDeg;
            double centerLatRad = centerLat * RadPerDeg;
            double y = EarthRadiusKM * Math.Log(Math.Tan(Math.PI / 4 + latRad / 2)) -
                        EarthRadiusKM * Math.Log(Math.Tan(Math.PI / 4 + centerLatRad / 2));

            int screenX = (int)(x * scale + width / 2 + panOffset.X);
            int screenY = (int)(-y * scale + height / 2 + panOffset.Y);
            return new SKPoint(screenX, screenY);
        }

        public static SKPoint ScreenToCoordinate(Size size, double scale, SKPoint panOffset, SKPoint point, double centerLat = 0, double centerLon = 0)
        {
            double x = (point.X - size.Width / 2 - panOffset.X) / scale;
            double y = -(point.Y - size.Height / 2 - panOffset.Y) / scale;

            double lat = (2 * Math.Atan(Math.Exp(y / EarthRadiusKM)) - Math.PI / 2) / RadPerDeg;
            double latRad = lat * RadPerDeg;

            double lon = centerLon + (x / (EarthRadiusKM * Math.Cos(latRad)));

            return new SKPoint((float)lat, (float)lon);
        }
    }
}
