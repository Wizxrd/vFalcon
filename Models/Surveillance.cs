using Newtonsoft.Json.Linq;
namespace vFalcon.Models;

public class Surveillance
{
    public string type { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public JObject crs { get; set; } = new JObject();
    public JArray features { get; set; } = new JArray();
}
