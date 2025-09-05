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
                string crcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "Profiles");
                var files = Directory.GetFiles(crcPath, "*.json");

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var profile = JsonConvert.DeserializeObject<Profile>(json);
                    if (profile == null) continue;

                    var jobj = JObject.Parse(json);
                    var displaySettings = jobj["DisplayWindowSettings"]?.FirstOrDefault()?["DisplaySettings"] as JArray;
                    if (displaySettings == null || !displaySettings.Any(s => s["$type"]?.ToString().Contains("Eram") == true)) continue;
                    list.Add(profile);
                    Logger.Info("ProfileService.LoadProfiles", $"Loaded ERAM Profile: \"{profile.Name}\"");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.LoadProfiles", ex.ToString());
            }
            return list;
        }

        public async Task Rename(string oldName, string newName)
        {
            try
            {

                string crcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "Profiles");
                string[] files = Directory.GetFiles(Loader.LoadFolder(crcPath), "*.json");
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
                string copy = $"{profile.Name} - Copy";
                string oldName = profile.Name;
                profile.Name = copy;
                string crcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "Profiles");
                string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                await Task.Run(() => File.WriteAllText(Loader.LoadFile(crcPath, $"{UniqueHash.Generate()}.json"), serialized));
                Logger.Info("ProfileService.Rename", $"Copied profile: \"{profile.Name}\" as: \"{copy}\"");
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
                    Filter = "JSON File (*.json)|*.json",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dialog.ShowDialog() == true)
                {
                    string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                    File.WriteAllText(dialog.FileName, serialized);
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
            string crcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "Profiles");
            string[] files = Directory.GetFiles(crcPath, "*.json");
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
                string filename = $"{profile.Id}.json"; // Or $"{profile.Id}.json"
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

        public async Task SaveAs(Profile profile, string name)
        {
            string[] files = Directory.GetFiles(Loader.LoadFolder("Profiles"), "*.json");
            foreach (string file in files)
            {
                try
                {
                    profile.Name = name;
                    string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                    Logger.Debug("E", UniqueHash.Generate());
                    await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{UniqueHash.Generate()}.json"), serialized));
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
