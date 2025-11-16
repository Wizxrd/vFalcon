namespace vFalcon.Models;
public class FilterSettings
{
    public bool Enabled { get; set; } = false;
    public bool RequireAll { get; set; } = false;
    public string Departure { get; set; } = string.Empty;
    public string Arrival { get; set; } = string.Empty;
    public string Sid { get; set; } = string.Empty;
    public string Star { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public int AltLow { get; set; } = 0;
    public int AltHigh { get; set; } = 0;
}
