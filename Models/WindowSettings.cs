namespace vFalcon.Models;

public class WindowSettings
{
    public string Bounds { get; set; } = "0,0,1250,850";
    public bool IsMaximized { get; set; } = false;
    public bool IsFullscreen { get; set; } = false;
    public bool ShowTitleBar { get; set; } = true;
}
