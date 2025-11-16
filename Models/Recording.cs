using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Models
{
    public class Recording
    {
        public JObject? FlightPlan { get; set; }
        public JArray Altitude { get; set; }
        public JArray GroundSpeed { get; set; }
        public JArray Heading { get; set; }
        public JArray FullDataBlock { get; set; }
        public JArray? History { get; set; }
        public JArray? Frequency {  get; set; }
        public int StartTick { get; set; }
    }
}
