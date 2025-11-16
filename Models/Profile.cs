using Newtonsoft.Json.Linq;
using vFalcon.Utils;
namespace vFalcon.Models;

public class Profile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUsedAt { get; set; }
    public string ArtccId { get; set; } = string.Empty;
    public GeneralSettings GeneralSettings { get; set; } = new();
    public MapSettings MapSettings { get; set; } = new();
    public FilterSettings FilterSettings { get; set; } = new();
    public DisplaySettings DisplaySettings { get; set; } = new();
    public WindowSettings WindowSettings { get; set; } = new();
    public AppearanceSettings AppearanceSettings { get; set; } = new();
    public JArray ActivePositions { get; set; } = new();
    public HashSet<string> ActiveEramFilters { get; set; } = new();
    public HashSet<string> ActiveStarsVideoMaps { get; set; } = new();

}
