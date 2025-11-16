using SkiaSharp;
using vFalcon.Renderables.Interfaces;
namespace vFalcon.Renderables;

public class Circle : IRenderable
{
    public SKPoint Center {  get; set; }
    public float Radius { get; set; }
    public SKPaint Paint { get; set; }
    public int ZIndex { get; set; }

    public Circle(SKPoint center, float radius, SKPaint paint, int zIndex)
    {
        Center = center;
        Radius = radius;
        Paint = paint;
        ZIndex = zIndex;
    }

    public void Dispose()
    {
        Paint?.Dispose();
    }
}
