using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using vFalcon.Models;
using vFalcon.Services.Interfaces;
using vFalcon.Utils;

namespace vFalcon.Services;

public class ArtccService : IArtccService
{
    public async Task<Artcc?> GetArtcc(string artccId)
    {
        try
        {
            string file = PathFinder.GetFilePath("ARTCCs", $"{artccId}.json");
            string json = await File.ReadAllTextAsync(file);
            var artcc = JsonConvert.DeserializeObject<Artcc>(json);
            return artcc;
        }
        catch (Exception ex)
        {
            Logger.Error("ArtccService.GetArtcc", ex.ToString());
            return null;
        }
    }

    public IEnumerable<string>? GetStarsPositions(Artcc artcc, string facilityId)
    {
        try
        {
            List<string> positions = new List<string>();
            List<string> starred = new List<string>();
            List<string> nonStarred = new List<string>();

            JArray childFacilities = (JArray)artcc.facility["childFacilities"];
            JObject match = childFacilities?.FirstOrDefault(cf => (string)cf["id"] == facilityId) as JObject;
            JArray starsConfig = match?["positions"] as JArray;
            foreach (var position in starsConfig)
            {
                string name = position["name"]?.ToString() ?? "Unknown";
                bool isStarred = position["starred"]?.ToObject<bool>() ?? false;
                long frequencyHz = position["frequency"]?.ToObject<long>() ?? 0;
                double frequencyMHz = frequencyHz / 1_000_000.0;
                string display = $"{name} - {frequencyMHz:F3}";
                if (isStarred)
                {
                    starred.Add(display);
                }
                else
                {
                    nonStarred.Add(display);
                }
            }
            positions.AddRange(starred);
            positions.AddRange(nonStarred);
            return positions;
        }
        catch (Exception ex)
        {
            Logger.Error("ArtccService.GetStarsPositions", ex.ToString());
            return null;
        }
    }

    public IEnumerable<string>? GetArtccPositions(Artcc artcc)
    {
        try
        {
            List<string> sectors = new List<string>();
            List<string> starred = new List<string>();
            List<string> nonStarred = new List<string>();

            JArray positions = artcc.facility["positions"] as JArray;
            if (positions == null) return Enumerable.Empty<string>();
            foreach (var position in positions)
            {
                string name = position["name"]?.ToString() ?? "Unknown";
                bool isStarred = position["starred"]?.ToObject<bool>() ?? false;
                long frequencyHz = position["frequency"]?.ToObject<long>() ?? 0;
                double frequencyMHz = frequencyHz / 1_000_000.0;
                string display = $"{name} - {frequencyMHz:F3}";
                if (isStarred)
                {
                    starred.Add(display);
                }
                else
                {
                    nonStarred.Add(display);
                }
            }
            sectors.AddRange(starred);
            sectors.AddRange(nonStarred);
            return sectors;
        }
        catch (Exception ex)
        {
            Logger.Error("ArtccService.GetArtccSectors", ex.ToString());
            return null;
        }
    }
}
