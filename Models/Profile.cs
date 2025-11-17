using Newtonsoft.Json.Linq;
namespace vFalcon.Models;

public class Profile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUsedAt { get; set; }
    public string ArtccId { get; set; } = string.Empty;


    public MainWindowSettings MainWindowSettings { get; set; } = new();
    public PositionsSettings PositionsSettings { get; set; } = new();
    public MapSettings MapSettings { get; set; } = new();
    public FilterSettings FilterSettings { get; set; } = new();
    public FindSettings FindSettings { get; set; } = new();
    public GeneralSettings GeneralSettings { get; set; } = new();
    public AppearanceSettings AppearanceSettings { get; set; } = new();
    public AircraftListSettings AircraftListSettings { get; set; } = new();

    public HashSet<string> ActiveEramFilters { get; set; } = new();
    public HashSet<string> ActiveStarsVideoMaps { get; set; } = new();

}
