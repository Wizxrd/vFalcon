namespace vFalcon.Models;

public class DisplaySettings
{
    public bool ShowToolbar { get; set; } = true;
    public bool ResizeBorder { get; set; } = false;
    public int ZoomIndex { get; set; } = 50;
    public Coordinate? Center { get; set; }
}
