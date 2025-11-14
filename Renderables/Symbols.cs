using Microsoft.VisualBasic.Logging;
using SkiaSharp;
using vFalcon.Utils;

namespace vFalcon.Renderables;

public class Symbols
{
    public static void Render(string style, SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        switch (style.ToLower())
        {
            case "obstruction1": Obstruction1(canvas, center, paint); break;
            case "obstruction2": Obstruction2(canvas, center, paint); break;
            case "helipad": Helipad(canvas, center, paint); break;
            case "nuclear": Nuclear(canvas, center, paint); break;
            case "emergencyairport": EmergencyAirport(canvas, center, paint); break;
            case "radar": Radar(canvas, center, paint); break;
            case "iaf": Iaf(canvas, center, paint); break;
            case "rnavonlywaypoint": RnavOnlyWaypoint(canvas, center, paint); break;
            case "rnav": Rnav(canvas, center, paint); break;
            case "airwayintersection": AirwayIntersection(canvas, center, paint); break;
            case "ndb": Ndb(canvas, center, paint); break;
            case "vor": Vor(canvas, center, paint); break;
            case "otherwaypoints": OtherWaypoints(canvas, center, paint); break;
            case "airport": Airport(canvas, center, paint); break;
            case "satelliteairport": SatelliteAirport(canvas, center, paint); break;
            case "tacan": Tacan(canvas, center, paint); break;
            case "dme": Tacan(canvas, center, paint); break;
            case "vci": Vci(canvas, center, paint); break;
            case "routeend": RouteEnd(canvas, center, paint); break;
        }
    }

    public static void RouteEnd(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 10f;
        float width = size / 1.5f;
        float height = size;

        SKPoint topLeft = new SKPoint(center.X - width, center.Y - height);
        SKPoint topRight = new SKPoint(center.X + width, center.Y - height);
        SKPoint bottomLeft = new SKPoint(center.X - width, center.Y + height);
        SKPoint bottomRight = new SKPoint(center.X + width, center.Y + height);

        using var path = new SKPath();

        path.MoveTo(topLeft);
        path.LineTo(center);
        path.LineTo(topRight);
        path.LineTo(topLeft);

        path.MoveTo(bottomLeft);
        path.LineTo(center);
        path.LineTo(bottomRight);

        canvas.Save();
        canvas.RotateDegrees(180, center.X, center.Y);
        canvas.DrawPath(path, paint);
        canvas.Restore();
    }

    public static void Obstruction1(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 6f;
        float width = size / 2;
        float height = size;

        SKPoint topLeft = new SKPoint(center.X - width, center.Y - height);
        SKPoint topRight = new SKPoint(center.X + width, center.Y - height);
        SKPoint bottomLeft = new SKPoint(center.X - width, center.Y + height);
        SKPoint bottomRight = new SKPoint(center.X + width, center.Y + height);

        using var path = new SKPath();

        path.MoveTo(topLeft);
        path.LineTo(center);
        path.LineTo(topRight);
        path.LineTo(topLeft);

        path.MoveTo(bottomLeft);
        path.LineTo(center);
        path.LineTo(bottomRight);

        canvas.DrawPath(path, paint);
    }

    public static void Obstruction2(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 10f;

        SKPoint dotCenter = center;
        float spread = size * 0.6f;

        SKPoint left = new SKPoint(center.X - spread, center.Y);
        SKPoint right = new SKPoint(center.X + spread, center.Y);
        SKPoint top = new SKPoint(center.X, center.Y - size);

        using var path = new SKPath();

        path.MoveTo(left);
        path.LineTo(top);
        path.LineTo(right);

        canvas.DrawPath(path, paint);

        float dotRadius = size * 0.15f;
        canvas.DrawCircle(dotCenter, dotRadius, paint);
    }

    public static void Helipad(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 15f;

        float radius = size / 2;

        canvas.DrawCircle(center, radius, paint);

        float hHeight = size * 0.6f;
        float hWidth = size * 0.3f;
        float hBarY = center.Y;

        SKPoint leftTop = new SKPoint(center.X - hWidth / 2, center.Y - hHeight / 2);
        SKPoint leftBottom = new SKPoint(center.X - hWidth / 2, center.Y + hHeight / 2);
        SKPoint rightTop = new SKPoint(center.X + hWidth / 2, center.Y - hHeight / 2);
        SKPoint rightBottom = new SKPoint(center.X + hWidth / 2, center.Y + hHeight / 2);

        canvas.DrawLine(leftTop, leftBottom, paint);
        canvas.DrawLine(rightTop, rightBottom, paint);

        canvas.DrawLine(new SKPoint(leftTop.X, hBarY), new SKPoint(rightTop.X, hBarY), paint);
    }

    public static void Nuclear(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        paint.Style = SKPaintStyle.Stroke;
        float size = 20f;

        float innerRadius = size * 0.1f;
        float bladeInner = size * 0.2f;
        float bladeOuter = size * 0.45f;
        float bladeAngle = 60f;

        canvas.DrawCircle(center, innerRadius, paint);

        float rotationOffset = -30f;

        for (int i = 0; i < 3; i++)
        {
            float angleStart = rotationOffset + i * 120f - (bladeAngle / 2f);
            float angleEnd = angleStart + bladeAngle;

            using var path = new SKPath();

            path.MoveTo(PointOnCircle(center, bladeInner, angleStart));
            path.ArcTo(new SKRect(center.X - bladeInner, center.Y - bladeInner, center.X + bladeInner, center.Y + bladeInner),
                       angleStart, bladeAngle, false);

            path.LineTo(PointOnCircle(center, bladeOuter, angleEnd));
            path.ArcTo(new SKRect(center.X - bladeOuter, center.Y - bladeOuter, center.X + bladeOuter, center.Y + bladeOuter),
                       angleEnd, -bladeAngle, false);

            path.Close();

            canvas.DrawPath(path, paint);
        }
    }

    private static SKPoint PointOnCircle(SKPoint center, float radius, float angleDeg)
    {
        float rad = angleDeg * (float)(Math.PI / 180);
        return new SKPoint(center.X + radius * (float)Math.Cos(rad),
                           center.Y + radius * (float)Math.Sin(rad));
    }

    public static void EmergencyAirport(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 3f;

        float extension = size * 0.8f;

        SKPoint topLeft = new SKPoint(center.X - size, center.Y - size);
        SKPoint topRight = new SKPoint(center.X + size, center.Y - size);
        SKPoint bottomRight = new SKPoint(center.X + size, center.Y + size);
        SKPoint bottomLeft = new SKPoint(center.X - size, center.Y + size);

        using var path = new SKPath();

        path.MoveTo(topLeft);
        path.LineTo(topRight);
        path.LineTo(bottomRight);
        path.LineTo(bottomLeft);
        path.Close();

        canvas.DrawPath(path, paint);

        canvas.DrawLine(topLeft, new SKPoint(topLeft.X - extension, topLeft.Y - extension), paint);

        canvas.DrawLine(bottomRight, new SKPoint(bottomRight.X + extension, bottomRight.Y + extension), paint);
    }

    public static void Radar(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 20f;

        float radius = size / 2;

        using var path = new SKPath();
        path.AddArc(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius), 180, -90);
        canvas.DrawPath(path, paint);

        float angleRad = 135f * (float)(Math.PI / 180f);
        SKPoint arcPoint = new SKPoint(
            center.X + radius * (float)Math.Cos(angleRad),
            center.Y + radius * (float)Math.Sin(angleRad)
        );

        float reverseRad = angleRad + (float)Math.PI;
        float sin = (float)Math.Sin(reverseRad);
        float cos = (float)Math.Cos(reverseRad);

        float dy = center.Y - arcPoint.Y;
        float lengthNeeded = dy / sin;
        SKPoint firstEnd = new SKPoint(
            arcPoint.X + lengthNeeded * cos,
            center.Y
        );

        SKPoint secondEnd = new SKPoint(firstEnd.X, arcPoint.Y);

        SKPoint thirdEnd = new SKPoint(
            secondEnd.X + lengthNeeded * cos,
            center.Y
        );

        canvas.DrawLine(arcPoint, firstEnd, paint);
        canvas.DrawLine(firstEnd, secondEnd, paint);
        canvas.DrawLine(secondEnd, thirdEnd, paint);
    }

    public static void Iaf(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 10f;

        float radius = size / 2;

        canvas.DrawCircle(center, radius, paint);

        SKPoint verticalTop = new SKPoint(center.X, center.Y - radius);
        SKPoint verticalBottom = new SKPoint(center.X, center.Y + radius);
        canvas.DrawLine(verticalTop, verticalBottom, paint);

        SKPoint horizontalLeft = new SKPoint(center.X - radius, center.Y);
        SKPoint horizontalRight = new SKPoint(center.X + radius, center.Y);
        canvas.DrawLine(horizontalLeft, horizontalRight, paint);
    }

    public static void RnavOnlyWaypoint(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 10f;

        float radius = size / 2;

        canvas.DrawCircle(center, radius, paint);

        SKPoint top = new SKPoint(center.X, center.Y - radius);
        SKPoint right = new SKPoint(center.X + radius, center.Y);
        SKPoint bottom = new SKPoint(center.X, center.Y + radius);
        SKPoint left = new SKPoint(center.X - radius, center.Y);

        using var path = new SKPath();
        path.MoveTo(top);
        path.LineTo(right);
        path.LineTo(bottom);
        path.LineTo(left);
        path.Close();

        canvas.DrawPath(path, paint);
    }

    public static void Rnav(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 5f;

        SKPoint topLeft = new SKPoint(center.X - size, center.Y - size);
        SKPoint topRight = new SKPoint(center.X + size, center.Y - size);
        SKPoint bottomRight = new SKPoint(center.X + size, center.Y + size);
        SKPoint bottomLeft = new SKPoint(center.X - size, center.Y + size);

        using var path = new SKPath();
        path.MoveTo(topLeft);
        path.LineTo(topRight);
        path.LineTo(bottomRight);
        path.LineTo(bottomLeft);
        path.Close();

        canvas.DrawPath(path, paint);
        canvas.DrawLine(new SKPoint(center.X, center.Y - size), new SKPoint(center.X, center.Y + size), paint);
        canvas.DrawLine(new SKPoint(center.X - size, center.Y), new SKPoint(center.X + size, center.Y), paint);
    }

    public static void AirwayIntersection(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 10f;

        float half = size / 2;
        float height = (float)(Math.Sqrt(3) / 2 * size);

        SKPoint top = new SKPoint(center.X, center.Y - height / 2);
        SKPoint bottomLeft = new SKPoint(center.X - half, center.Y + height / 2);
        SKPoint bottomRight = new SKPoint(center.X + half, center.Y + height / 2);

        using var path = new SKPath();
        path.MoveTo(top);
        path.LineTo(bottomRight);
        path.LineTo(bottomLeft);
        path.Close();

        canvas.DrawPath(path, paint);
    }

    public static void Ndb(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 10f;

        float radiusX = size / 3;
        float radiusY = size / 2;

        SKRect ovalRect = new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY);

        canvas.DrawOval(ovalRect, paint);

        SKPoint lowerLeft = new SKPoint(ovalRect.Left, ovalRect.Bottom);
        SKPoint upperRight = new SKPoint(ovalRect.Right, ovalRect.Top);
        canvas.DrawLine(lowerLeft, upperRight, paint);
    }

    public static void Vor(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 10f;

        float radiusX = size / 3;
        float radiusY = size / 2;

        SKRect ovalRect = new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY);

        canvas.DrawOval(ovalRect, paint);
    }

    public static void OtherWaypoints(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 5f;
        canvas.DrawLine(new SKPoint(center.X, center.Y - size), new SKPoint(center.X, center.Y + size), paint);
        canvas.DrawLine(new SKPoint(center.X - size, center.Y), new SKPoint(center.X + size, center.Y), paint);
    }

    public static void Airport(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 3f;

        SKPoint topLeft = new SKPoint(center.X - size, center.Y - size);
        SKPoint topRight = new SKPoint(center.X + size, center.Y - size);
        SKPoint bottomRight = new SKPoint(center.X + size, center.Y + size);
        SKPoint bottomLeft = new SKPoint(center.X - size, center.Y + size);

        using var path = new SKPath();
        path.MoveTo(topLeft);
        path.LineTo(topRight);
        path.LineTo(bottomRight);
        path.LineTo(bottomLeft);
        path.Close();

        canvas.DrawPath(path, paint);
    }

    public static void SatelliteAirport(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 5f;

        SKPoint bottomLeft = new SKPoint(center.X - size, center.Y + size / 3f);
        SKPoint topLeft = new SKPoint(center.X - size, center.Y - size);
        SKPoint topRight = new SKPoint(center.X + size, center.Y - size);

        canvas.DrawLine(bottomLeft, topLeft, paint);

        canvas.DrawLine(topLeft, topRight, paint);
    }

    public static void Tacan(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 10f;

        float radiusX = size / 3;
        float radiusY = size / 2;

        SKRect ovalRect = new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY);

        //canvas.DrawOval(ovalRect, paint);
        float dotRadius = size * 0.1f;
        //canvas.DrawCircle(center, dotRadius, paint);
    }

    public static void Vci(SKCanvas canvas, SKPoint center, SKPaint paint)
    {
        float size = 5f;
        SKPath path = new();
        SKPoint bottomRight = new SKPoint(center.X + size, center.Y + size);
        SKPoint bottomLeft = new SKPoint(center.X - size, center.Y + size);
        SKPoint topLeft = new SKPoint(center.X - size, center.Y - size);
        path.MoveTo(bottomRight);
        path.LineTo(bottomLeft);
        path.LineTo(topLeft);
        canvas.DrawPath(path, paint);

        SKPath arcPath = new();
        arcPath.AddArc(new SKRect(center.X - size, center.Y - size, center.X + size, center.Y + size), -90, 90);
        arcPath.AddArc(new SKRect(center.X - size/2, center.Y - size / 2, center.X + size / 2, center.Y + size / 2), -90, 90);
        canvas.DrawPath(arcPath, paint);
    } // FIXME
}
