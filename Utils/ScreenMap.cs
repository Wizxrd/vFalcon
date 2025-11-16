using SkiaSharp;
using System.Drawing;
using vFalcon.Models;

namespace vFalcon.Utils;

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

    public static Coordinate ScreenToCoordinate(Size size, double scale, SKPoint panOffset, SKPoint point)
    {
        double x = (point.X - size.Width / 2 - panOffset.X) / scale;
        double y = -(point.Y - size.Height / 2 - panOffset.Y) / scale;

        double latRad = 2 * Math.Atan(Math.Exp(y / EarthRadiusKM)) - Math.PI / 2;
        double lat = latRad / RadPerDeg;
        double lon = x / EarthRadiusKM / RadPerDeg;
        return new Coordinate{ Lat = lat, Lon = lon };
    }

    public static double DistanceInNM(Coordinate coord1, Coordinate coord2)
    {
        double lat1 = coord1.Lat * Math.PI / 180.0;
        double lon1 = coord1.Lon * Math.PI / 180.0;
        double lat2 = coord2.Lat * Math.PI / 180.0;
        double lon2 = coord2.Lon * Math.PI / 180.0;

        double dLat = lat2 - lat1;
        double dLon = lon2 - lon1;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

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

    public static bool PointInPolygon(SKPoint p, IList<SKPoint> coordinates)
    {
        bool inside = false;
        for (int i = 0, j = coordinates.Count - 1; i < coordinates.Count; j = i++)
        {
            var a = coordinates[i];
            var b = coordinates[j];
            if (((a.Y > p.Y) != (b.Y > p.Y)) &&
                (p.X < (b.X - a.X) * (p.Y - a.Y) / (b.Y - a.Y) + a.X))
                inside = !inside;
        }
        return inside;
    }

    public static bool PointInCoordinatePolygon(SKPoint pilotPosition, List<Coordinate> coordinates, int w, int h, double scale, SKPoint pan)
    {
        if (coordinates.Count < 3) return false;
        var ring = new List<SKPoint>(coordinates.Count);
        foreach (Coordinate coord in coordinates)
        {
            ring.Add(ScreenMap.CoordinateToScreen(w, h, scale, pan, coord.Lat, coord.Lon));
        }
        return PointInPolygon(pilotPosition, ring);
    }

    public static Coordinate ProjectPointOnBearing(double latDeg, double lonDeg, double bearingDeg, double distanceNm)
    {
        double φ1 = latDeg * ScreenMap.RadPerDeg;
        double λ1 = lonDeg * ScreenMap.RadPerDeg;
        double θ = bearingDeg * ScreenMap.RadPerDeg;
        double δ = distanceNm / ScreenMap.EarthRadiusNM;

        double sinφ2 = Math.Sin(φ1) * Math.Cos(δ) + Math.Cos(φ1) * Math.Sin(δ) * Math.Cos(θ);
        double φ2 = Math.Asin(sinφ2);
        double y = Math.Sin(θ) * Math.Sin(δ) * Math.Cos(φ1);
        double x = Math.Cos(δ) - Math.Sin(φ1) * sinφ2;
        double λ2 = λ1 + Math.Atan2(y, x);

        double lat2 = φ2 / ScreenMap.RadPerDeg;
        double lon2 = (λ2 / ScreenMap.RadPerDeg + 540) % 360 - 180;
        return new Coordinate{ Lat=lat2, Lon = lon2 };
    }
}
