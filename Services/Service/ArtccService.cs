using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Interfaces;
using vFalcon.ViewModels;
using static vFalcon.Nasr.Models.StarCsvDataModel;

namespace vFalcon.Services.Service
{
    public class ArtccService : IArtccService
    {
        public async Task<Artcc>? LoadArtcc(string artccId)
        {
            try
            {
                string file = Loader.LoadFile("ARTCCs", $"{artccId}.json");
                var json = await File.ReadAllTextAsync(file);
                var artcc = JsonConvert.DeserializeObject<Artcc>(json);
                return artcc;
            }
            catch (Exception ex)
            {
                Logger.Error("ArtccService.LoadArtcc", ex.ToString());
            }
            return null;
        }

        public static IEnumerable<string> GetStarsPositions(EramViewModel eramViewModel)
        {
            var positions = new List<string>();
            var starred = new List<string>();
            var nonStarred = new List<string>();

            var childFacilities = (JArray)eramViewModel.artcc.facility["childFacilities"];
            var match = childFacilities?.FirstOrDefault(cf => (string)cf["id"] == eramViewModel.profile.FacilityId) as JObject;
            var starsConfig = match?["positions"] as JArray;
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

        public static IEnumerable<string> GetArtccSectors(string artccId)
        {
            var sectors = new List<string>();
            var starred = new List<string>();
            var nonStarred = new List<string>();

            string path = Loader.LoadFile("ARTCCs", $"{artccId}.json");
            var file = File.ReadAllText(path);
            JObject json = JObject.Parse(file);

            var positions = json["facility"]["positions"] as JArray;

            if (positions == null)  return Enumerable.Empty<string>();

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
    }
}
