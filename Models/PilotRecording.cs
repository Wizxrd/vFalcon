using Newtonsoft.Json.Linq;
namespace vFalcon.Models;

public class PilotRecording
{
    public JObject FlightPlan { get; set; } = new();
    public List<int> Altitude { get; set; } = new();
    public List<int> GroundSpeed { get; set; } = new();
    public List<int> Heading { get; set; } = new();
    public List<bool> FullDataBlock { get; set; } = new();
    public List<List<double>> History { get; set; } = new();
    public List<string> Frequency { get; set; } = new();
    public int StartTick { get; set; } = new();
}
