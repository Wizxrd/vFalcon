using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace vFalcon.Models;

public class VideoMap
{
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public JArray tags { get; set; } = new JArray();
    public string shortName { get; set; } = string.Empty;
    public string sourceFileName { get; set; } = string.Empty;
    public DateTime lastUpdatedAt { get; set; } = DateTime.UtcNow;
    public string starsBrightnessCategory { get; set; } = string.Empty;
    public int starsId { get; set; }
    public bool starsAlwaysVisible { get; set; }
    public bool tdmOnly { get; set; }

    public static JArray GetGeoMaps()
    {
        JObject eramConfig = Artcc.GetEramConfiguration();
        return (JArray)eramConfig["geoMaps"];
    }

    public static List<VideoMap> GetVideoMaps()
    {
        List<VideoMap> videoMaps = new();
        foreach (JObject videoMap in App.Artcc.videoMaps)
        {
            videoMaps.Add(GetVideoMapFromJObject(videoMap));
        }
        return videoMaps;
    }

    public static JArray GetFaciltiyVideoMaps(string facilityId)
    {
        JArray videoMaps = new();
        foreach (JObject child in App.Artcc.facility["childFacilities"])
        {
            if ((string)child["id"] != facilityId) continue;
            JObject starsConfiguration = (JObject)child["starsConfiguration"];
            if (starsConfiguration == null) continue;
            JArray videoMapIds = (JArray)starsConfiguration["videoMapIds"];
            var allowedIds = new HashSet<string>(videoMapIds.Select(t => (string)t));
            var selectedVideoMaps = App.Artcc.videoMaps
                .Where(vm => allowedIds.Contains((string)vm["id"]))
                .Cast<JObject>();
            foreach (JObject videoMap in selectedVideoMaps)
            {
                videoMaps.Add((string)videoMap["id"]);
            }
            break;
        }
        return videoMaps;
    }

    public static Dictionary<string, string> GetEramFilters(string geoMap)
    {
        Dictionary<string, string> filters = new();
        JObject eramConfig = Artcc.GetEramConfiguration();
        JArray geoMaps = (JArray)eramConfig["geoMaps"];
        foreach (JObject geoMapSet in geoMaps)
        {
            if ((string)geoMapSet["name"] != geoMap) continue;
            JArray filterMenu = (JArray)geoMapSet["filterMenu"];
            for (int i = 0; i < filterMenu.Count; i++)
            {
                var filter = (JObject)filterMenu[i];
                string name = $"{(string)filter["labelLine1"]} {(string)filter["labelLine2"]}";
                if (string.IsNullOrWhiteSpace(name)) continue;
                filters.Add((i+1).ToString(), name);
            }
            break;
        }
        return filters;
    }

    public static List<string> GetEramVideoMapIds()
    {
        List<string> videoMaps = new();
        JObject eramConfig = Artcc.GetEramConfiguration();
        JArray geoMaps = (JArray)eramConfig["geoMaps"];
        foreach (JObject geoMap in geoMaps)
        {
            JArray geoMapVideoMaps = (JArray)geoMap["videoMapIds"];
            for (int i = 0; i < geoMapVideoMaps.Count; i++)
            {
                videoMaps.Add((string)geoMapVideoMaps[i]);
            }
            break;
        }
        return videoMaps;
    }

    public static List<string> GetMapsetVideoMapIds(string name)
    {
        List<string> videoMaps = new();
        JObject eramConfig = Artcc.GetEramConfiguration();
        JArray geoMaps = (JArray)eramConfig["geoMaps"];
        foreach (JObject geoMap in geoMaps)
        {
            if ((string)geoMap["name"] != name) continue;
            JArray geoMapVideoMaps = (JArray)geoMap["videoMapIds"];
            for (int i = 0; i < geoMapVideoMaps.Count; i++)
            {
                videoMaps.Add((string)geoMapVideoMaps[i]);
            }
            break;
        }
        return videoMaps;
    }

    public static List<VideoMap> GetChildFacilityVideoMaps(string facilityId)
    {
        List<VideoMap> videoMaps = new();
        foreach (JObject child in App.Artcc.facility["childFacilities"])
        {
            if ((string)child["id"] != facilityId) continue;
            JObject starsConfiguration = (JObject)child["starsConfiguration"];
            if (starsConfiguration == null) continue;
            JArray videoMapIds = (JArray)starsConfiguration["videoMapIds"];
            var allowedIds = new HashSet<string>(videoMapIds.Select(t => (string)t));
            var selectedVideoMaps = App.Artcc.videoMaps
                .Where(vm => allowedIds.Contains((string)vm["id"]))
                .Cast<JObject>();
            foreach (JObject videoMap in selectedVideoMaps)
            {
                videoMaps.Add(VideoMap.GetVideoMapFromJObject(videoMap));
            }
            break;
        }
        return videoMaps;
    }

    public static VideoMap GetVideoMapFromJObject(JObject videoMap)
    {
        string json = videoMap.ToString(Newtonsoft.Json.Formatting.Indented);
        return JsonConvert.DeserializeObject<VideoMap>(json);
    }
}
