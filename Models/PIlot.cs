using Newtonsoft.Json.Linq;

namespace vFalcon.Models
{
    public class Pilot
    {
        public int CID { get; set; }
        public string? Name { get; set; }
        public string? Callsign { get; set; }
        public string? Server { get; set; }
        public int PilotRating { get; set; }
        public int MilitaryRating { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Altitude { get; set; }
        public int GroundSpeed { get; set; }
        public string? Transponder { get; set; }
        public int Heading { get; set; }
        public double QNHinHG { get; set; }
        public int QNHmb { get; set; }
        public JObject? FlightPlan { get; set; }
        public DateTime LogOnTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool FullDataBlock { get; set; }
    }
}
