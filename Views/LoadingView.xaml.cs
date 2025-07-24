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
            TextBlockLoading.Text = "Importing Artccs";
            await ImportArtccs();
            TextBlockLoading.Text = "Importing Profiles";
            await ImportEramProfiles();
            LoadProfileView loadProfileView = new LoadProfileView();
            this.Close();
            loadProfileView.ShowDialog();
        }

        private async Task ImportArtccs()
        {
            try
            {
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

                        if (DateTime.TryParse(existingTimestamp, out var existingTime) &&
                            DateTime.TryParse(newTimestamp, out var incomingTime) &&
                            incomingTime <= existingTime)
                        {
                            Logger.Info("LoadingView.ImportArtccs", $"Skipping up to date ARTCC: \"{id}\"");
                            continue;
                        }
                    }

                    await File.WriteAllTextAsync(destinationPath, incoming.ToString(Formatting.Indented));
                    Logger.Info("LoadingView.ImportArtccs", $"Imported ARTCC: \"{id}\"");
                }
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
                string crcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "Profiles");
                var files = Directory.GetFiles(crcPath, "*.json");

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var profile = JsonConvert.DeserializeObject<Profile>(json);
                    if (profile == null) continue;

                    var jobj = JObject.Parse(json);
                    var displaySettings = jobj["DisplayWindowSettings"]?.FirstOrDefault()?["DisplaySettings"] as JArray;

                    if (displaySettings == null) continue;
                    if (!displaySettings.Any(s => s["$type"]?.ToString().Contains("Eram") == true)) continue;

                    string filename = $"{profile.Name}.json";
                    string destinationPath = Loader.LoadFile("Profiles", filename);

                    await File.WriteAllTextAsync(destinationPath, JsonConvert.SerializeObject(profile, Formatting.Indented));
                    Logger.Info("LoadingView.ImportEramProfiles", $"Imported ERAM Profile: \"{profile.Name}\"");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("LoadingView.ImportEramProfiles", ex.ToString());
            }
        }
    }
}
