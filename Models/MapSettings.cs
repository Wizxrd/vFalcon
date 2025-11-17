namespace vFalcon.Models;

public class MapSettings
{
    public string Facility { get; set; } = string.Empty;
    public string GeoMap { get; set; } = string.Empty;
    public WindowSettings WindowSettings { get; set; } = new(400, 100, 300, 400);
}
