using vFalcon.Utils;
namespace vFalcon.Models
{
    public class GeneralSettings
    {
        public bool TopDown { get; set; } = false;
        public bool VideoMapPreProcess { get; set; } = false;
        public bool AutoDatablock { get; set; } = true;
        public DatablockType DefaultDatablockType { get; set; } = DatablockType.Eram;
        public int LogLevel { get; set; } = 3;
        public WindowSettings WindowSettings { get; set; } = new(650,500,250,200);
    }
}
