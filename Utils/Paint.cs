using SkiaSharp;
using vFalcon.Models;
namespace vFalcon.Utils;

public class Paint
{
    private static readonly SKTypeface FontTypeFace = SKTypeface.FromFile(PathFinder.GetFilePath("Resources/Fonts", "ERAM.ttf"));
    public static SKPaint VideoMapLine(ProcessedFeature f, byte rgb)
    {
        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            IsAntialias = true,
            StrokeWidth = f.AppliedAttributes.TryGetValue("thickness", out var th) ? System.Convert.ToSingle(th) : 1f,
            Color = new SKColor(rgb, rgb, rgb)
        };
        if (f.AppliedAttributes.TryGetValue("style", out var style) && style != null)
        {
            var s = style.ToString().ToLowerInvariant();
            if (s == "shortdashed") paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 20 }, 0);
            else if (s == "longdashed") paint.PathEffect = SKPathEffect.CreateDash(new float[] { 20, 30 }, 0);
            else if (s == "longdashshortdash") paint.PathEffect = SKPathEffect.CreateDash(new float[] { 10, 20, 10, 20 }, 0);
        }
        return paint;
    }

    public static SKPaint VideoMapText(byte rgb, float fontSize)
    {
        return new SKPaint
        {
            Typeface = FontTypeFace,
            TextSize = fontSize <= 0 ? 12f : fontSize,
            Color = new SKColor(rgb, rgb, rgb),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
    }

    public static SKPaint VideoMapSymbol(byte rgb)
    {
        return new SKPaint
        {
            Color = new SKColor(rgb, rgb, rgb),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
    }

    public static SKPaint Text(SKColor color, float textSize, SKPaintStyle style, float stroke = 0f)
    {
        {
            return new SKPaint
            {
                Color = color,
                Typeface = FontTypeFace,
                TextSize = textSize,
                IsAntialias = true,
                Style = style,
                StrokeWidth = stroke
            };
        }
    }

    public static SKPaint Line(SKColor color, SKPaintStyle style, float stroke)
    {
        return new SKPaint
        {
            Color = color,
            Style = style,
            StrokeWidth = stroke,
            IsAntialias = true
        };
    }

    public static SKPaint Rect(SKColor color, SKPaintStyle style, float stroke)
    {
        return new SKPaint
        {
            Color = color,
            Style = style,
            StrokeWidth = stroke,
            IsAntialias = true
        };
    }

    public static SKPaint Symbol(SKColor color, SKPaintStyle style, float stroke)
    {
        return new SKPaint
        {
            Color = color,
            Style = style,
            StrokeWidth = stroke,
            IsAntialias = true
        };
    }

    public static SKPaint Circle(SKColor color, SKPaintStyle style, float stroke)
    {
        return new SKPaint
        {
            Color = color,
            Style = style,
            StrokeWidth = stroke,
            IsAntialias = true
        };
    }
}
