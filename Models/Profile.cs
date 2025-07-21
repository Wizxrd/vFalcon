using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace vFalcon.Models
{
    public class Profile
    {
        public string Artcc { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ArtccId { get; set; } = string.Empty;
        public string SectorName { get; set; } = string.Empty;
        public string SectorFreq { get; set; } = string.Empty;

        public string AppchBright { get; set; } = "50";
        public string LowsBright { get; set; } = "75";
        public string HighsBright { get; set; } = "100";
        public string BndryBright { get; set; } = "100";

        public bool BndryEnabled { get; set; } = true;
        public bool AppchCntlEnabled { get; set; } = false;
        public bool LowsEnabled { get; set; } = false;
        public bool HighsEnabled { get; set; } = false;

        public string CursorSize { get; set; } = "2";
    }
}
