using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vFalcon.Models;
using vFalcon.Helpers;
using vFalcon.Services.Interfaces;

namespace vFalcon.Services.Service
{
    public class ProfileService : IProfileService
    {
        public async Task<List<Profile>> LoadProfiles()
        {
            List<Profile> list = new List<Profile>();
            try
            {
                string profilesPath = Loader.LoadFolder("Profiles");
                var files = Directory.GetFiles(profilesPath, "*.json");

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var profile = JsonConvert.DeserializeObject<Profile>(json);
                    if (profile == null) continue;
                    list.Add(profile);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.LoadProfiles", ex.ToString());
            }
            return list;
        }

        public async Task New(string name, string artccId, string facilityId, string displayType)
        {
            Profile profile = new()
            {
                Id = UniqueHash.Generate(),
                Name = name,
                LastUsedAt = DateTime.UtcNow,
                ArtccId = artccId,
                FacilityId = facilityId,
                DisplayType = displayType,
                ActiveGeoMap = null,
                TopDown = false,
                LogLevel = 3,
                RecordAudio = false,
                PttKey = null,
                ZoomRange = 500,
                Center = null,
                WindowSettings = new JObject
                {
                    { "Bounds", "50,50,1280,720" },
                    { "IsMaximized", false },
                    { "IsFullscreen", false },
                    { "ShowTitleBar", true }
                },
                AppearanceSettings = new JObject
                {
                    { "Background", 2 },
                    { "Backlight", 100 },
                    { "DatablockFontSize", 12 },
                    { "FullDatablockBrightness", 100 },
                    { "LimitedDatablockBrightness", 50 },
                    { "MapBrightness", 100 },
                    { "HistoryBrightness", 25 },
                    { "HistoryLength", 5 },
                    { "VectorLength", 1 },
                },
                MapFilters = new JObject()
            };
            string filePath = Path.Combine(Loader.LoadFolder("Profiles"), $"{profile.Id}.json");
            string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, serialized);
        }

        public async Task<bool> Import()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Import Profile",
                Filter = "vFalcon Profile (*.falcon|*.falcon",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(dialog.FileName);
                    Profile? imported = JsonConvert.DeserializeObject<Profile>(json);

                    if (imported != null)
                    {
                        imported.Id = UniqueHash.Generate();
                        string filePath = Path.Combine(Loader.LoadFolder("Profiles"), $"{imported.Id}.json");
                        string serialized = JsonConvert.SerializeObject(imported, Formatting.Indented);
                        await File.WriteAllTextAsync(filePath, serialized);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("ProfileService.Import", ex.ToString());
                }
            }
            return false;
        }

        public async Task Rename(string oldName, string newName)
        {
            try
            {

                string profilesPath = Loader.LoadFolder("Profiles");
                var files = Directory.GetFiles(profilesPath, "*.json");
                foreach (string file in files)
                {
                    JObject profile = JObject.Parse(File.ReadAllText(file));
                    if ((string?)profile["Name"] == oldName)
                    {
                        profile["Name"] = newName;
                        string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                        await Task.Run(() => File.WriteAllText(file, serialized));
                        Logger.Info("ProfileService.Rename", $"Renamed \"{oldName}\" to \"{newName}\"");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.Rename", ex.ToString());
            }
        }

        public async Task Copy(Profile profile)
        {
            try
            {
                Profile copy = profile;
                copy.Id = UniqueHash.Generate();
                copy.Name = $"{profile.Name} - Copy";
                string serialized = JsonConvert.SerializeObject(copy, Formatting.Indented);
                await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{copy.Id}.json"), serialized));
                Logger.Debug("ProfileService.Rename", $"Copied profile: \"{profile.Name}\" as: \"{copy.Name}\"");
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.Copy", ex.ToString());
            }
        }

        public void Export(Profile profile)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Export Profile",
                    Filter = "vFalcon Profile (*.falcon)|*.falcon",
                    DefaultExt = ".falcon",
                    AddExtension = true,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dialog.ShowDialog() == true)
                {
                    var fileName = dialog.FileName;
                    if (!fileName.EndsWith(".falcon", StringComparison.OrdinalIgnoreCase))
                        fileName += ".falcon";
                    string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                    File.WriteAllText(fileName, serialized);
                    Logger.Info("ProfileService.Export", $"Exported profile: \"{profile.Name}\" to: \"{dialog.FileName}\"");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.Export", ex.ToString());
            }
        }

        public async Task Delete(Profile profile)
        {
            string profilesPath = Loader.LoadFolder("Profiles");
            string[] files = Directory.GetFiles(profilesPath, "*.json");
            foreach (string file in files)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(file);
                    JObject jObj = JObject.Parse(content);
                    if ((string?)jObj["Name"] == profile.Name)
                    {
                        await Task.Run(() => File.Delete(file));
                        Logger.Info("ProfileService.Delete", $"Deleted profile: \"{profile.Name}\"");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("ProfileService.Delete", $"Error with {file}: {ex.Message}");
                }
            }
        }

        public void Save(Profile profile)
        {
            try
            {
                string filename = $"{profile.Id}.json";
                string path = Loader.LoadFile("Profiles", filename);
                string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                File.WriteAllText(path, serialized);
                Logger.Debug("ProfileService.Save", $"Saved profile: \"{profile.Name}\"");
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.Save", ex.ToString());
            }
        }

        public async Task SaveAsync(Profile profile)
        {
            try
            {
                string filename = $"{profile.Id}.json";
                string path = Loader.LoadFile("Profiles", filename);
                string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                await File.WriteAllTextAsync(path, serialized);
                Logger.Debug("ProfileService.Save", $"Saved profile: \"{profile.Name}\"");
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.Save", ex.ToString());
            }
        }

        public async Task SaveAs(Profile profile, string name)
        {
            string[] files = Directory.GetFiles(Loader.LoadFolder("Profiles"), "*.json");
            foreach (string file in files)
            {
                try
                {
                    profile.Id = UniqueHash.Generate();
                    profile.Name = name;
                    string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                    await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{profile.Id}.json"), serialized));
                    Logger.Debug("ProfileService.SaveAs", $"Saved profile as: \"{profile.Name}\"");
                    return;
                }
                catch (Exception ex)
                {
                    Logger.Error("ProfileService.SaveAs", $"Error with {file}: {ex.Message}");
                }
            }
        }
    }
}
