using Newtonsoft.Json.Linq;

namespace vFalcon.Models
{
    public class Profile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime LastUsedAt { get; set; }
        public string ArtccId { get; set; } = string.Empty;
        public string FacilityId { get; set; } = string.Empty;
        public string DisplayType { get; set; } = string.Empty;
        public string ActiveGeoMap { get; set; } = string.Empty;
        public bool TopDown { get; set; } = false;
        public int LogLevel { get; set; }
        public bool RecordAudio { get; set; } = false;
        public int? PttKey { get; set; } = 0;
        public int ZoomRange { get; set; } = 0;
        public JObject Center { get; set; } = new JObject();
        public JObject WindowSettings { get; set; } = new JObject();
        public JObject AppearanceSettings { get; set; } = new JObject();
        public JObject MapFilters { get; set; } = new JObject();
    }
}
