using Newtonsoft.Json.Linq;
using vFalcon.Utils;
namespace vFalcon.Models;

public class Artcc
{
    public string id { get; set; } = string.Empty;
    public string lastUpdatedAt { get; set; } = string.Empty;
    public JObject facility { get; set; } = new JObject();
    public JArray visibilityCenters { get; set; } = new JArray();
    public string aliasesLastUpdatedAt { get; set; } = string.Empty;
    public JArray videoMaps { get; set; } = new JArray();
    public JArray transceivers { get; set; } = new JArray();
    public JArray autoAtcRules { get; set; } = new JArray();

    public static List<string> GetFacilityIds()
    {
        List<string> facilities = new();
        Artcc artcc = App.Artcc;
        JObject facility = (JObject)artcc.facility;
        string artccId = (string)facility["id"];
        string name = (string)facility["name"];
        JArray childFacilities = (JArray)facility["childFacilities"];
        facilities.Add($"{artccId} - {name}");
        foreach (JObject child in childFacilities)
        {
            facilities.Add($"{child["id"]}");
        }
        return facilities;
    }

    public static List<string> GetFacilities()
    {
        List<string> facilities = new();
        Artcc artcc = App.Artcc;
        JObject facility = (JObject)artcc.facility;
        string artccId = (string)facility["id"];
        string name = (string)facility["name"];
        JArray childFacilities = (JArray)facility["childFacilities"];
        facilities.Add($"{artccId} - {name}");
        foreach (JObject child in childFacilities)
        {
            facilities.Add($"{child["id"]} - {child["name"]}");
        }
        return facilities;
    }

    public static string GetFacilityIdFromName(string name)
    {
        return name.Substring(0, 3);
    }

    public static JObject GetEramConfiguration()
    {
        JObject facility = (JObject)App.Artcc.facility;
        return (JObject)facility["eramConfiguration"];
    }

    private static List<string> GetPositionDisplayNames(JArray positions)
    {
        List<string> displayNames = new();
        foreach (JObject position in positions)
        {
            string frequencyMhz = Frequency.ConvertToMhz((int)position["frequency"]);
            displayNames.Add($"{position["name"]} - {frequencyMhz}");
        }
        return displayNames;
    }

    public static List<string> GetPositions(string facilityId)
    {
        List<string> positions = new();
        JObject facility = (JObject)App.Artcc.facility;
        string artccId = (string)facility["id"];
        if (artccId == facilityId)
        {
            JArray facilityPositions = (JArray)facility["positions"];
            positions = GetPositionDisplayNames(facilityPositions);
            return positions;
        }
        else
        {
            foreach (JObject child in App.Artcc.facility["childFacilities"])
            {
                if ((string)child["id"] != facilityId) continue;
                JArray childPositions = (JArray)child["positions"];
                List<string> displayNames = GetPositionDisplayNames(childPositions);
                displayNames.ForEach(p => positions.Add(p));
                break;
            }
            return positions;
        }
    }
}
