namespace vFalcon.Models
{
    public class MainWindowSettings
    {
        public DisplaySettings DisplaySettings { get; set; } = new();
        public WindowSettings WindowSettings { get; set; } = new(0, 0, 1300, 900);
    }
}
