using Newtonsoft.Json.Linq;

namespace vFalcon.Models
{
    public class Profile
    {
        public string Id { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastUsedAt { get; set; } = string.Empty;
        public string ArtccId { get; set; } = string.Empty;
        public string LastUsedEnvironment { get; set; } = string.Empty;
        public string LastUsedPositionId { get; set; } = string.Empty;
        public string NetowrkRating { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ControllerInfo { get; set; } = string.Empty;

        public JArray DisplayWindowSettings { get; set; } = new JArray();
        public JObject ControllerListSettings { get; set; } = new JObject();
        public JObject FlightPlanEditorSettings { get; set; } = new JObject();
        public JObject MessagesAreaSettings { get; set; } = new JObject();
        public JObject VoiceSwitchSettings { get; set; } = new JObject();

        public JArray Bookmarks { get; set; } = new JArray();
        public JArray SelectedBeaconCodes { get; set; } = new JArray();
        public bool? InvertNumericKeypad { get; set; } = null;
        public JArray SecondaryVoiceSwitchPositionIds { get; set; } = new JArray();

        public string ActivatedSectorName { get; set; } = string.Empty;
        public string ActivatedSectorFreq {  get; set; } = string.Empty;
    }
}
