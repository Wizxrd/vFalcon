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
        public static double EarthRadiusNM = 3440.065;
        private static double EarthRadiusKM = 6371.0;
        public static double RadPerDeg = Math.PI / 180.0;
        public static double Deg2Rad(double d) => d * Math.PI / 180.0;

        public static SKPoint CoordinateToScreen(int width, int height, double scale, SKPoint panOffset, double lat, double lon)
        {
            double x = EarthRadiusKM * lon * RadPerDeg;

            double latRad = lat * RadPerDeg;
            double y = EarthRadiusKM * Math.Log(Math.Tan(Math.PI / 4 + latRad / 2));

            int screenX = (int)(x * scale + width / 2 + panOffset.X);
            int screenY = (int)(-y * scale + height / 2 + panOffset.Y);
            return new SKPoint(screenX, screenY);
        }

        public static SKPoint ScreenToCoordinate(Size size, double scale, SKPoint panOffset, SKPoint point)
        {
            double x = (point.X - size.Width / 2 - panOffset.X) / scale;
            double y = -(point.Y - size.Height / 2 - panOffset.Y) / scale;

            double latRad = 2 * Math.Atan(Math.Exp(y / EarthRadiusKM)) - Math.PI / 2;
            double lat = latRad / RadPerDeg;
            double lon = x / EarthRadiusKM / RadPerDeg;

            return new SKPoint((float)lat, (float)lon);
        }

        public static double DistanceInNM(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;

            lat1 *= Math.PI / 180.0;
            lat2 *= Math.PI / 180.0;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusNM * c;
        }

        public static double NormalizeAngle(double deg)
        {
            deg %= 360.0;
            if (deg < 0) deg += 360.0;
            return deg;
        }

        public static double AngleDelta(double a, double b)
        {
            double d = NormalizeAngle(b - a);
            if (d > 180.0) d -= 360.0;
            return d;
        }

        public static double BearingTo(double lat1, double lon1, double lat2, double lon2)
        {
            double rlat1 = lat1 * Math.PI / 180.0;
            double rlat2 = lat2 * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;

            double y = Math.Sin(dLon) * Math.Cos(rlat2);
            double x = Math.Cos(rlat1) * Math.Sin(rlat2) - Math.Sin(rlat1) * Math.Cos(rlat2) * Math.Cos(dLon);
            double brng = Math.Atan2(y, x) * 180.0 / Math.PI;
            return NormalizeAngle(brng);
        }
    }
}
