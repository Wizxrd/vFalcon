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

namespace vFalcon.Services
{
    public class ProfileService : IProfileService
    {
        public List<Profile> LoadProfiles()
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
                    Logger.Error("ProfileService.LoadProfiles", ex.ToString());
                }
            }
            return list;
        }

        public async Task New(string artcc, string name)
        {
            var profile = new JObject
            {
                { "Artcc", artcc },
                { "Name", name },
                { "ArtccId", artcc.Split('-').Last().Trim() }
            };

            string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
            await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{UniqueHash.Generate()}.json"), serialized));
            Logger.Debug("ProfileService.New", $"Created Profile: \"{name}\"");
        }

        public async Task Rename(string oldName, string newName)
        {
            try
            {
                string[] files = Directory.GetFiles(Loader.LoadFolder("Profiles"), "*.json");
                foreach (string file in files)
                {
                    JObject profile = JObject.Parse(File.ReadAllText(file));
                    if ((string?)profile["Name"] == oldName)
                    {
                        profile["Name"] = newName;
                        string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                        await Task.Run(() => File.WriteAllText(file, serialized));
                        Logger.Debug("ProfileService.Rename", $"Renamed \"{oldName}\" to \"{newName}\"");
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
                var copy = new Profile
                {
                    Name = $"{profile.Name} - Copy",
                    Artcc = profile.Artcc,
                    ArtccId = profile.ArtccId,
                    LastSectorName = profile.LastSectorName,
                    LastSectorFreq = profile.LastSectorFreq,
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
                await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{UniqueHash.Generate()}.json"), serialized));
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
                    Filter = "JSON File (*.json)|*.json",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dialog.ShowDialog() == true)
                {
                    string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                    File.WriteAllText(dialog.FileName, serialized);
                    Logger.Debug("ProfileService.Export", $"Exported profile: \"{profile.Name}\" to: \"{dialog.FileName}\"");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ProfileService.Export", ex.ToString());
            }
        }

        public async Task Delete(Profile profile)
        {
            string[] files = Directory.GetFiles(Loader.LoadFolder("Profiles"), "*.json");
            foreach (string file in files)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(file);
                    JObject jObj = JObject.Parse(content);
                    if ((string?)jObj["Name"] == profile.Name)
                    {
                        await Task.Run(() => File.Delete(file));
                        Logger.Debug("ProfileService.Delete", $"Deleted profile: \"{profile.Name}\"");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("ProfileService.Delete", $"Error with {file}: {ex.Message}");
                }
            }
        }

        public async Task Import(string file)
        {
            var profile = JObject.Parse(File.ReadAllText(file));
            string profileName = profile["Name"]?.ToString() ?? "Unnamed";
            string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
            await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", $"{UniqueHash.Generate()}.json"), serialized));
            Logger.Debug("ProfileService.Import", $"Imported profile: \"{profileName}\"");
        }

        public async Task Save(Profile profile)
        {
            string[] files = Directory.GetFiles(Loader.LoadFolder("Profiles"), "*.json");
            foreach (string file in files)
            {
                try
                {
                    string content = await File.ReadAllTextAsync(file);
                    JObject jObj = JObject.Parse(content);
                    if ((string?)jObj["Name"] == profile.Name)
                    {
                        string serialized = JsonConvert.SerializeObject(profile, Formatting.Indented);
                        await Task.Run(() => File.WriteAllText(Loader.LoadFile("Profiles", file), serialized));
                        Logger.Debug("ProfileService.Save", $"Saved profile: \"{profile.Name}\"");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("ProfileService.Save", $"Error with {file}: {ex.Message}");
                }
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
