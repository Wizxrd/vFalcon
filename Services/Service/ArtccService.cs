using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Interfaces;
using Newtonsoft.Json;

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
    }
}
