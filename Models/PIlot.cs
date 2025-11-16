using Newtonsoft.Json.Linq;
using vFalcon.Utils;
namespace vFalcon.Models;

public class Pilot
{
    public int CID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Callsign { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Altitude { get; set; }
    public int GroundSpeed { get; set; }
    public string Transponder { get; set; } = string.Empty;
    public int Heading { get; set; }
    public JObject FlightPlan { get; set; } = new JObject();
    public DateTime LogOnTime { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string CwtCode { get; set; } = string.Empty;
    public DatablockType DatablockType { get; set; } = DatablockType.Eram;
    public bool ForcedDatablockType { get; set; } = false;
    public bool FullDatablockEnabled { get; set; } = false;
    public bool ForcedFullDatablock { get; set; } = false;
    public bool Filtered { get; set; } = false;
    public DatablockPosition DatablockPosition { get; set; } = DatablockPosition.Default;
    public int LeaderLineLength { get; set; } = 1;
    public bool DwellEmphasisEnabled { get; set; } = false;
    public bool DriEnabled { get; set; } = false;
    public int DriSize { get; set; } = 5;
    public List<Coordinate> History { get; set; } = new();
    public bool DisplayFiledRoute { get; set; } = false;
    public bool DisplayFullRoute { get; set; } = false;

    public string StarsDatablockRow2Col1 = string.Empty;
    public string StarsDatablockRow2Col2 = string.Empty;
}
