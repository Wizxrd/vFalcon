using SkiaSharp;
using vFalcon.Renderables.Interfaces;
namespace vFalcon.Renderables;

public class Rect : IRenderable
{
    public SKRect skRect { get; set; }
    public SKPaint Paint { get; set; }
    public int ZIndex { get; set; }

    public Rect(SKRect rect, SKPaint paint, int zIndex)
    {
        skRect = rect;
        Paint = paint;
        ZIndex = zIndex;
    }

    public void Dispose()
    {
        Paint?.Dispose();
    }
}
