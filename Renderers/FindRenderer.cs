using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Helpers;

namespace vFalcon.Renderers
{
    public class FindRenderer
    {
        private static readonly SKColor FindColor = SKColor.Parse("#32CD32");
        private readonly SKPaint findPaint;
        private const float squareSize = 10f;
        public int RenderCount = 0;

        public FindRenderer()
        {
            findPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = FindColor,
                IsAntialias = true
            };
        }

        public void RenderFind(SKCanvas canvas, Size size, double scale, SKPoint panOffset, List<double> coords)
        {
            SKPoint screenPoint = ScreenMap.CoordinateToScreen(size.Width, size.Height, scale, panOffset, coords[0], coords[1]);
            canvas.DrawRect(screenPoint.X - squareSize / 2, screenPoint.Y - squareSize / 2, squareSize, squareSize, findPaint);
        }
    }
}
