using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Helpers
{
    public class Symbols
    {
        // size = 10
        public static void Obstruction1(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            SKPoint topLeft = new SKPoint(center.X - size, center.Y - size);
            SKPoint topRight = new SKPoint(center.X + size, center.Y - size);
            SKPoint bottomLeft = new SKPoint(center.X - size, center.Y + size);
            SKPoint bottomRight = new SKPoint(center.X + size, center.Y + size);
            SKPoint middle = center;

            using var path = new SKPath();

            path.MoveTo(topLeft);
            path.LineTo(middle);
            path.LineTo(topRight);
            path.LineTo(topLeft);

            path.MoveTo(bottomLeft);
            path.LineTo(middle);
            path.LineTo(bottomRight);

            canvas.DrawPath(path, paint);
        }
        // size = 10
        public static void Obstruction2(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

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

            using var dotPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            float dotRadius = size * 0.15f;
            canvas.DrawCircle(dotCenter, dotRadius, dotPaint);
        }

        // size = 15
        public static void Helipad(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

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

        // size = 25
        public static void Nuclear(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            using var fillPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            float innerRadius = size * 0.1f;
            float bladeInner = size * 0.2f;
            float bladeOuter = size * 0.45f;
            float bladeAngle = 60f;

            canvas.DrawCircle(center, innerRadius, fillPaint);

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

                canvas.DrawPath(path, fillPaint);
            }
        }

        // get a point on a circle at angle (degrees) goes with Nuclear
        private static SKPoint PointOnCircle(SKPoint center, float radius, float angleDeg)
        {
            float rad = angleDeg * (float)(Math.PI / 180);
            return new SKPoint(center.X + radius * (float)Math.Cos(rad),
                               center.Y + radius * (float)Math.Sin(rad));
        }

        // size = 10
        public static void EmergencyAirport(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

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

        // size = 20
        public static void Radar(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            float radius = size / 2;

            using var path = new SKPath();
            path.AddArc(new SKRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius),
                        180, -90);
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

        // size = 10
        public static void Iaf(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            float radius = size / 2;

            canvas.DrawCircle(center, radius, paint);

            SKPoint verticalTop = new SKPoint(center.X, center.Y - radius);
            SKPoint verticalBottom = new SKPoint(center.X, center.Y + radius);
            canvas.DrawLine(verticalTop, verticalBottom, paint);

            SKPoint horizontalLeft = new SKPoint(center.X - radius, center.Y);
            SKPoint horizontalRight = new SKPoint(center.X + radius, center.Y);
            canvas.DrawLine(horizontalLeft, horizontalRight, paint);
        }

        // size = 10 - this one is hard to see
        public static void RnavOnlyWaypoint(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

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

        // size = 5
        public static void Rnav(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

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

        // size = 10
        public static void AirwayIntersection(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

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

        // size = 10
        public static void Ndb(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            float radiusX = size / 3;
            float radiusY = size / 2;

            SKRect ovalRect = new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY);

            canvas.DrawOval(ovalRect, paint);

            SKPoint lowerLeft = new SKPoint(ovalRect.Left, ovalRect.Bottom);
            SKPoint upperRight = new SKPoint(ovalRect.Right, ovalRect.Top);
            canvas.DrawLine(lowerLeft, upperRight, paint);
        }

        // size = 10
        public static void Vor(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColor.Parse("#f3f3f3"),
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            float radiusX = size / 3;
            float radiusY = size / 2;

            SKRect ovalRect = new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY);

            canvas.DrawOval(ovalRect, paint);
        }

        // size = 10
        public static void OtherWaypoints(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColor.Parse("#f3f3f3"),
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            canvas.DrawLine(new SKPoint(center.X, center.Y - size), new SKPoint(center.X, center.Y + size), paint);

            canvas.DrawLine(new SKPoint(center.X - size, center.Y), new SKPoint(center.X + size, center.Y), paint);
        }

        // size = 5
        public static void Airport(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

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

        // size = 5
        public static void SatelliteAirport(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            SKPoint bottomLeft = new SKPoint(center.X - size, center.Y + size/3f);
            SKPoint topLeft = new SKPoint(center.X - size, center.Y - size);
            SKPoint topRight = new SKPoint(center.X + size, center.Y - size);

            canvas.DrawLine(bottomLeft, topLeft, paint);

            canvas.DrawLine(topLeft, topRight, paint);
        }

        public static void Tacan(SKCanvas canvas, SKPoint center, float size)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            float radiusX = size / 3;
            float radiusY = size / 2;

            SKRect ovalRect = new SKRect(center.X - radiusX, center.Y - radiusY, center.X + radiusX, center.Y + radiusY);

            canvas.DrawOval(ovalRect, paint);

            using var dotPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            float dotRadius = size * 0.1f;
            canvas.DrawCircle(center, dotRadius, dotPaint);
        }
    }
}
