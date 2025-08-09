using AdonisUI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for LoadingView.xaml
    /// </summary>
    public partial class LoadingView : AdonisWindow
    {
        public LoadingView()
        {
            InitializeComponent();
            InitializeImports();
        }

        public async void InitializeImports()
        {
            Logger.Info("LoadingView.InitializeImports", "Starting");
            TextBlockLoading.Text = "Importing Artccs";
            await ImportArtccs();
            TextBlockLoading.Text = "Importing Profiles";
            await ImportEramProfiles();
            Logger.Info("LoadingView.InitializeImports", "Completed");
            LoadProfileView loadProfileView = new LoadProfileView();
            this.Close();
            loadProfileView.ShowDialog();
        }

        private async Task ImportArtccs()
        {
            try
            {
                Logger.Info("LoadingView.ImportArtccs", "Starting");
                string sourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "ARTCCs");
                var files = Directory.GetFiles(sourcePath, "*.json");

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var incoming = JsonConvert.DeserializeObject<JObject>(json);
                    if (incoming == null || incoming["id"] == null) continue;

                    string id = incoming["id"].ToString();
                    string newTimestamp = incoming["lastUpdatedAt"]?.ToString() ?? "";

                    string filename = $"{id}.json";
                    string destinationPath = Loader.LoadFile("ARTCCs", filename);

                    if (File.Exists(destinationPath))
                    {
                        var existingJson = await File.ReadAllTextAsync(destinationPath);
                        var existing = JsonConvert.DeserializeObject<JObject>(existingJson);
                        string existingTimestamp = existing?["lastUpdatedAt"]?.ToString() ?? "";

                        if (DateTime.TryParse(existingTimestamp, out var existingTime) && DateTime.TryParse(newTimestamp, out var incomingTime) && incomingTime <= existingTime)
                        {
                            TextBlockLoading.Text = $"Skipped ARTCC: \"{id}\"";
                            Logger.Debug("LoadingView.ImportArtccs", $"Skipped up to date ARTCC: \"{id}\"");
                            continue;
                        }
                    }

                    await File.WriteAllTextAsync(destinationPath, incoming.ToString(Formatting.Indented));
                    TextBlockLoading.Text = $"Imported ARTCC: \"{id}\"";
                    Logger.Debug("LoadingView.ImportArtccs", $"Imported ARTCC: \"{id}\"");
                }
                Logger.Info("LoadingView.ImportArtccs", "Completed");
            }
            catch (Exception ex)
            {
                Logger.Error("LoadingView.ImportArtccs", ex.ToString());
            }
        }

        private async Task ImportEramProfiles()
        {
            try
            {
                // clear folder
                string folderPath = Loader.LoadFolder("Profiles");
                foreach (var file in Directory.GetFiles(folderPath))
                {
                    File.Delete(file);
                }

                Logger.Info("LoadingView.ImportEramProfiles", "Starting");
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

                    string filename = $"{profile.Id}.json";
                    string destinationPath = Loader.LoadFile("Profiles", filename);

                    /*var incomingLastUsedAtStr = jobj["LastUsedAt"]?.ToString() ?? "";
                    if (File.Exists(destinationPath))
                    {
                        var existingJson = await File.ReadAllTextAsync(destinationPath);
                        var existing = JsonConvert.DeserializeObject<JObject>(existingJson);
                        var existingLastUsedAtStr = existing?["LastUsedAt"]?.ToString() ?? "";

                        if (DateTime.TryParse(existingLastUsedAtStr, out var existingTime) && DateTime.TryParse(incomingLastUsedAtStr, out var incomingTime) && incomingTime <= existingTime)
                        {
                            TextBlockLoading.Text = $"Skipped profile: \"{profile.Name}\"";
                            Logger.Debug("LoadingView.ImportEramProfiles", $"Skipped up to date profile: \"{profile.Name}\"");
                            continue;
                        }
                    }*/

                    await File.WriteAllTextAsync(destinationPath, JsonConvert.SerializeObject(profile, Formatting.Indented));
                    TextBlockLoading.Text = $"Imported profile: \"{profile.Name}\"";
                    Logger.Debug("LoadingView.ImportEramProfiles", $"Imported ERAM Profile: \"{profile.Name}\"");
                }
                Logger.Info("LoadingView.ImportEramProfiles", "Completed");
            }
            catch (Exception ex)
            {
                Logger.Error("LoadingView.ImportEramProfiles", ex.ToString());
            }
        }
    }
}
