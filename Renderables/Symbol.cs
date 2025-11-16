using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Renderables.Interfaces;
using vFalcon.Utils;

namespace vFalcon.Renderables
{
    public class Symbol : IRenderable
    {
        public string Style { get; set; }
        public SKPoint Center { get; set; }
        public SKPaint Paint { get; set; }
        public int ZIndex { get; set; }

        public Symbol(string style, SKPoint center, SKPaint paint, int zIndex)
        {
            Style = style;
            Center = center;
            Paint = paint;
            ZIndex = zIndex;
        }

        public void Dispose()
        {
            Paint?.Dispose();
        }
    }
}
