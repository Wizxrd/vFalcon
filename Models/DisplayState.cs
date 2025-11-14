using SkiaSharp;
using System.Drawing;
namespace vFalcon.Models;

public class DisplayState
{
    public double Scale { get; set; }
    public SKPoint PanOffset { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public Size Size { get; set; }
    public Coordinate Center { get; set; } = new();
    public bool ZoomOnMouse { get; set; }
    public bool IsPanning { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsReady { get; set; } = false;
}
