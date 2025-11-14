using Newtonsoft.Json.Linq;
using vFalcon.Utils;
namespace vFalcon.Models;

public class Profile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUsedAt { get; set; }
    public string ArtccId { get; set; } = string.Empty;
    public MapSettings MapSettings { get; set; } = new();
    public bool TopDown { get; set; } = false;
    public bool VideoMapPreProcess { get; set; } = false;
    public int LogLevel { get; set; } = 3;
    public bool AutoDatablock { get; set; } = true;
    public DatablockType DefaultDatablockType = DatablockType.Eram;
    public int ZoomIndex { get; set; } = 50;
    public Coordinate? Center { get; set; }
    public WindowSettings WindowSettings { get; set; } = new();
    public AppearanceSettings AppearanceSettings { get; set; } = new();
    public JArray ActivePositions { get; set; } = new();
    public HashSet<string> ActiveEramFilters { get; set; } = new();
    public HashSet<string> ActiveStarsVideoMaps { get; set; } = new();

}
