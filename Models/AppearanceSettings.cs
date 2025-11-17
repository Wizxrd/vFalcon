using vFalcon.Utils;
namespace vFalcon.Models;

public class AppearanceSettings
{
    public int Background { get; set; } = 0;
    public int Backlight { get; set; } = 100;
    public DatablockType DatablockType { get; set; } = DatablockType.Eram;
    public int DatablockFontSize { get; set; } = 12;
    public int FullDatablockBrightness { get; set; } = 100;
    public int LimitedDatablockBrightness { get; set; } = 90;
    public int MapBrightness { get; set; } = 75;
    public int HistoryBrightness { get; set; } = 25;
    public int HistoryLength { get; set; } = 5;
    public int VelocityVector { get; set; } = 1;
    public WindowSettings WindowSettings { get; set; } = new(650, 700, 250, -1);
}
