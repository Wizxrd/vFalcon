using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace vFalcon.Models
{
    public class Profile
    {
        public string Artcc { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ArtccId { get; set; } = string.Empty;
        public string SectorName { get; set; } = string.Empty;
        public string SectorFreq { get; set; } = string.Empty;

        public string AppchBright { get; set; } = "50";
        public string LowsBright { get; set; } = "75";
        public string HighsBright { get; set; } = "100";
        public string BndryBright { get; set; } = "100";

        public bool BndryEnabled { get; set; } = true;
        public bool AppchCntlEnabled { get; set; } = false;
        public bool LowsEnabled { get; set; } = false;
        public bool HighsEnabled { get; set; } = false;

        public static List<Profile> LoadProfiles()
        {
            var list = new List<Profile>();
            string folder = Loader.LoadFolder("Profiles");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var files = Directory.GetFiles(folder, "*.json");
            foreach (var file in files)
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var profile = JsonConvert.DeserializeObject<Profile>(json);

                    if (profile != null)
                    {
                        list.Add(profile);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Profile.LoadProfiles", ex.ToString());
                }
            }
            return list;
        }

        public async static Task New(string artcc, string name)
        {
            JObject profile = new JObject
            {
                { "Artcc", artcc },
                { "Name", name },
                { "ArtccId",  artcc.Split('-').Last().Trim() }
            };
            string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
            await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{Helper.GenerateUniqueHash()}.json"), serialized));
            Logger.Debug("Profile.New", $"Profile: \"{name}\" Successfully Created");
        }

        public static async Task Rename(string oldName, string newName)
        {
            try
            {
                string[] files = Directory.GetFiles(Loader.LoadFolder("Profiles"), "*.json");
                foreach (string file in files)
                {
                    JObject profile = JObject.Parse(File.ReadAllText(Loader.LoadFile("Profiles", file)));
                    string profileName = profile["Name"]?.ToString() ?? string.Empty;
                    if (profileName == oldName)
                    {
                        profile["Name"] = newName;
                        string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                        await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{Path.GetFileName(file)}"), serialized));
                        Logger.Debug("Profile.Load", $"Profile: \"{oldName}\" Successfully Renamed To: \"{newName}\"");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Profile.Rename", ex.ToString());
            }
        }

        public static async Task Copy(Profile profile)
        {
            try
            {
                var copy = new Profile
                {
                    Name = $"{profile.Name} - Copy",
                    Artcc = profile.Artcc,
                    ArtccId = profile.ArtccId,
                    SectorName = profile.SectorName,
                    SectorFreq = profile.SectorFreq,
                    AppchBright = profile.AppchBright,
                    LowsBright = profile.LowsBright,
                    HighsBright = profile.HighsBright,
                    BndryBright = profile.BndryBright,
                    BndryEnabled = profile.BndryEnabled,
                    AppchCntlEnabled = profile.AppchCntlEnabled,
                    LowsEnabled = profile.LowsEnabled,
                    HighsEnabled = profile.HighsEnabled
                };

                string serialized = JsonConvert.SerializeObject(copy, Formatting.Indented);
                await Task.Run(() =>
                    File.WriteAllText(Loader.LoadFile("Profiles", $"{Helper.GenerateUniqueHash()}.json"), serialized));
            }
            catch (Exception ex)
            {
                Logger.Error("Profile.Copy", ex.ToString());
            }
        }

        public static void Export(Profile profile)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Export Profile",
                    Filter = "JSON File (*.json)|*.json",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    string path = saveFileDialog.FileName;
                    string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                    File.WriteAllText(path, serialized);
                    Logger.Debug("Export", $"Profile: \"{profile.Name}\" Successfully Exported To: {path}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Profile.Export", ex.ToString());
            }
        }

        public static async Task Delete(Profile profile)
        {
            string[] files = Directory.GetFiles(Loader.LoadFolder("Profiles"), "*.json");

            foreach (string file in files)
            {
                try
                {
                    string fileContent = await Task.Run(() => File.ReadAllText(file));
                    JObject jObject = JObject.Parse(fileContent);
                    string profileName = jObject["Name"]?.ToString() ?? string.Empty;

                    if (profileName == profile.Name)
                    {
                        if (true)
                        {
                            await Task.Run(() => File.Delete(file));
                            Logger.Debug("Delete", $"Profile: \"{profile.Name}\" Successfully Deleted");
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("LoadProfileViewModel.DeleteAsync", $"Error processing file {file}: {ex.Message}");
                }
            }
        }

        public static async Task Import(string file)
        {
            JObject profile = JObject.Parse(File.ReadAllText(file));
            string profileName = profile["Name"]?.ToString() ?? string.Empty;
            string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
            await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{Helper.GenerateUniqueHash()}.json"), serialized));
            Logger.Debug("Profile.Import", $"Profile: \"{profileName}\" Successfully Imported");
        }
    }
}
