using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Models
{
    public class Artcc
    {
        public string id { get; set; } = string.Empty;
        public string lastUpdatedAt { get; set; } = string.Empty;
        public JObject facility { get; set; } = new JObject();
        public JArray visibilityCenters { get; set; } = new JArray();
        public string aliasesLastUpdatedAt {  get; set; } = string.Empty;
        public JArray videoMaps { get; set; } = new JArray();
        public JArray transceivers { get; set; } = new JArray();
        public JArray autoAtcRules { get; set; } = new JArray();
    }
}
