using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using vFalcon.Helpers;
using vFalcon.Views;

namespace vFalcon.ViewModels
{
    public class LoadingViewModel : ViewModelBase
    {
        public Action Close;
        private string progressContent = string.Empty;
        private int progressValue = 0;
        private int totalSteps = 0;
        private int completedSteps = 0;

        public string ProgressContent
        {
            get => progressContent;
            set { progressContent = value; OnPropertyChanged(); }
        }

        public int ProgressValue
        {
            get => progressValue;
            set { progressValue = value; OnPropertyChanged(); }
        }

        public LoadingViewModel()
        {
            InitializeImports();
        }

        private async void InitializeImports()
        {
            try
            {
                var artccToImport = await CountArtccsToImport();
                var profilesToImport = await CountProfilesToImport();

                totalSteps = artccToImport + profilesToImport;
                completedSteps = 0;
                ProgressValue = 0;

                await ImportArtccs(artccToImport);
                await ImportProfiles(profilesToImport);
                ProgressValue = 100;
                ProgressContent = "Launching vFalcon";
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                LoadProfilesView loadProfileView = new LoadProfilesView();
                Close.Invoke();
                loadProfileView.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.Error("LoadingView.InitializeImports", ex.ToString());
            }
        }

        private void IncrementProgress()
        {
            completedSteps++;
            if (totalSteps <= 0) { ProgressValue = 0; return; }
            var pct = (int)Math.Round(100.0 * completedSteps / totalSteps);
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;
            ProgressValue = pct;
        }

        private async Task<int> CountArtccsToImport()
        {
            try
            {
                string sourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "ARTCCs");
                if (!Directory.Exists(sourcePath)) return 0;
                var files = Directory.GetFiles(sourcePath, "*.json");
                int count = 0;

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var incoming = JsonConvert.DeserializeObject<JObject>(json);
                    if (incoming == null || incoming["id"] == null) continue;

                    string id = incoming["id"].ToString();
                    string newTimestamp = incoming["lastUpdatedAt"]?.ToString() ?? "";
                    string destinationPath = Loader.LoadFile("ARTCCs", $"{id}.json");
                    count++;
                }
                return count;
            }
            catch (Exception ex)
            {
                Logger.Error("LoadingView.CountArtccsToImport", ex.ToString());
                return 0;
            }
        }

        private async Task<int> CountProfilesToImport()
        {
            try
            {
                string crcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "Profiles");
                if (!Directory.Exists(crcPath)) return 0;
                var files = Directory.GetFiles(crcPath, "*.json");
                int count = 0;

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var jobj = JObject.Parse(json);
                    var displaySettings = jobj["DisplayWindowSettings"]?.FirstOrDefault()?["DisplaySettings"] as JArray;
                    if (displaySettings == null) continue;
                    if (displaySettings.Any(s => s["$type"]?.ToString().Contains("Eram") == true))
                        count++;
                }
                return count;
            }
            catch (Exception ex)
            {
                Logger.Error("LoadingView.CountProfilesToImport", ex.ToString());
                return 0;
            }
        }

        private async Task ImportArtccs(int expectedCount)
        {
            try
            {
                Logger.Info("LoadingView.ImportArtccs", "Starting");
                string sourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "ARTCCs");
                if (!Directory.Exists(sourcePath)) { ProgressContent = "No ARTCC source folder"; return; }
                var files = Directory.GetFiles(sourcePath, "*.json");

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var incoming = JsonConvert.DeserializeObject<JObject>(json);
                    if (incoming == null || incoming["id"] == null) continue;

                    string id = incoming["id"].ToString();
                    string newTimestamp = incoming["lastUpdatedAt"]?.ToString() ?? "";
                    string destinationPath = Loader.LoadFile("ARTCCs", $"{id}.json");

                    if (File.Exists(destinationPath))
                    {
                        var existingJson = await File.ReadAllTextAsync(destinationPath);
                        var existing = JsonConvert.DeserializeObject<JObject>(existingJson);
                        string existingTimestamp = existing?["lastUpdatedAt"]?.ToString() ?? "";

                        if (DateTime.TryParse(existingTimestamp, out var existingTime) &&
                            DateTime.TryParse(newTimestamp, out var incomingTime) &&
                            incomingTime <= existingTime)
                        {
                            ProgressContent = $"Skipped ARTCC: \"{id}\"";
                            IncrementProgress();
                            Logger.Debug("LoadingView.ImportArtccs", $"Skipped up to date ARTCC: \"{id}\"");
                            continue;
                        }
                    }

                    await File.WriteAllTextAsync(destinationPath, incoming.ToString(Formatting.Indented));
                    ProgressContent = $"Imported ARTCC: \"{id}\"";
                    IncrementProgress();
                    Logger.Debug("LoadingView.ImportArtccs", $"Imported ARTCC: \"{id}\"");
                }

                if (expectedCount == 0) IncrementProgress();
                Logger.Info("LoadingView.ImportArtccs", "Completed");
            }
            catch (Exception ex)
            {
                Logger.Error("LoadingView.ImportArtccs", ex.ToString());
            }
        }

        private async Task ImportProfiles(int expectedCount)
        {
            try
            {
                string folderPath = Loader.LoadFolder("Profiles");
                if (Directory.Exists(folderPath))
                {
                    foreach (var file in Directory.GetFiles(folderPath))
                        File.Delete(file);
                }

                Logger.Info("LoadingView.ImportEramProfiles", "Starting");
                string crcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CRC", "Profiles");
                if (!Directory.Exists(crcPath)) { ProgressContent = "No Profiles source folder"; return; }
                var files = Directory.GetFiles(crcPath, "*.json");

                foreach (var file in files)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var jobj = JObject.Parse(json);
                    var displaySettings = jobj["DisplayWindowSettings"]?.FirstOrDefault()?["DisplaySettings"] as JArray;
                    if (displaySettings == null || !displaySettings.Any(s => s["$type"]?.ToString().Contains("Eram") == true)) continue;

                    var profile = JsonConvert.DeserializeObject<JObject>(json);
                    if (profile == null) continue;

                    string filename = $"{profile["Id"]}.json";
                    string destinationPath = Loader.LoadFile("Profiles", filename);

                    await File.WriteAllTextAsync(destinationPath, JsonConvert.SerializeObject(profile, Formatting.Indented));
                    ProgressContent = $"Imported profile: \"{profile["Name"]}\"";
                    Logger.Debug("LoadingView.ImportEramProfiles", $"Imported ERAM Profile: \"{profile["Name"]}\"");
                    IncrementProgress();
                }

                if (expectedCount == 0) IncrementProgress();
                Logger.Info("LoadingView.ImportEramProfiles", "Completed");
            }
            catch (Exception ex)
            {
                Logger.Error("LoadingView.ImportEramProfiles", ex.ToString());
            }
        }
    }
}
