namespace vFalcon.Models;

public class WindowSettings
{
    public bool IsOpen { get; set; } = true;
    public string Bounds { get; set; }
    public bool IsMaximized { get; set; } = false;
    public bool IsFullscreen { get; set; } = false;
    public bool ShowTitleBar { get; set; } = true;

    public WindowSettings(int left, int top, int width, int height)
    {
        Bounds = $"{left},{top},{width},{height}";
    }
}
