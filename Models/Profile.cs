using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.ConstrainedExecution;

namespace vFalcon.Models
{
    public class Profile
    {
        public string Name { get; set; } = string.Empty;
        public string Artcc { get; set; } = string.Empty;
        public string ArtccId { get; set; } = string.Empty;
        public string LastSectorName { get; set; } = string.Empty;
        public string LastSectorFreq { get; set; } = string.Empty;

        public string AppchBright { get; set; } = "50";
        public string LowsBright { get; set; } = "75";
        public string HighsBright { get; set; } = "100";
        public string BndryBright { get; set; } = "100";

        public bool BndryEnabled { get; set; } = true;
        public bool AppchCntlEnabled { get; set; } = false;
        public bool LowsEnabled { get; set; } = false;
        public bool HighsEnabled { get; set; } = false;

        public string CursorSize { get; set; } = "2";

        public int WindowWidth { get; set; } = 1080;
        public int WindowHeight { get; set; } = 720;
        public int WindowTop { get; set; } = 50;
        public int WindowLeft { get; set; } = 50;
        public bool IsMaximized { get; set; } = false;

        public double MapCenterLat { get; set; }
        public double MapCenterLon { get; set; }
    }
}
