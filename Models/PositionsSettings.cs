using Newtonsoft.Json.Linq;

namespace vFalcon.Models;

public class PositionsSettings
{
    public WindowSettings WindowSettings { get; set; } = new(100,100, 300, 400);
    public JArray ActivePositions { get; set; } = new();
}
