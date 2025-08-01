using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Models
{
    public class ProcessedFeature
    {
        public string GeometryType; // "Line", "Symbol", "Text"
        public object Geometry;     // Position[] or MultiLine or Point
        public Dictionary<string, object> AppliedAttributes; // Merged props: style, size, etc.
    }
}
