using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Controls;
using vFalcon.Models;
using vFalcon.Utils;
namespace vFalcon.Updaters;

public class vNas
{
    private static readonly HttpClient http = new HttpClient();

    public static async Task CheckForUpdates(TextBlock TextBlockLoading)
    {
        try
        {
            Logger.Info("vNas.CheckForUpdates", "Starting");

            var folder = PathFinder.GetFolderPath("ARTCCs");
            Directory.CreateDirectory(folder);
            var files = Directory.GetFiles(folder, "*.json");

            foreach (var file in files)
            {
                string artccId = Path.GetFileNameWithoutExtension(file);
                string url = $"https://data-api.vnas.vatsim.net/api/artccs/{artccId}";

                using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                resp.EnsureSuccessStatusCode();
                string jsonText = await resp.Content.ReadAsStringAsync();

                var incoming = JsonConvert.DeserializeObject<JObject>(jsonText);
                if (incoming == null || incoming["id"] == null) continue;

                string id = incoming["id"]!.ToString();
                string newTimestamp = incoming["lastUpdatedAt"]?.ToString() ?? string.Empty;

                string destinationPath = PathFinder.GetFilePath("ARTCCs", $"{id}.json");

                await UpdateVideoMaps(JsonConvert.DeserializeObject<Artcc>(jsonText), TextBlockLoading);

                if (File.Exists(destinationPath))
                {
                    var existingJson = await File.ReadAllTextAsync(destinationPath);
                    var existing = JsonConvert.DeserializeObject<JObject>(existingJson);
                    string existingTimestamp = existing?["lastUpdatedAt"]?.ToString() ?? string.Empty;

                    if (DateTimeOffset.TryParse(existingTimestamp, out var existingTime) &&
                        DateTimeOffset.TryParse(newTimestamp, out var incomingTime) &&
                        incomingTime <= existingTime)
                    {
                        TextBlockLoading.Text = $"\"{id}\" up to date";
                        Logger.Info("vNas.UpdateArtccs", $"\"{id}\" up to date");
                        continue;
                    }
                }
                await File.WriteAllTextAsync(destinationPath, incoming.ToString(Formatting.Indented));
                TextBlockLoading.Text = $"Updated ARTCC: \"{id}\"";
                Logger.Info("vNas.CheckForUpdates", $"Updated ARTCC: \"{id}\"");
            }
            Logger.Info("vNas.CheckForUpdates", "Completed");
        }
        catch (Exception ex)
        {
            Logger.Error("vNas.CheckForUpdates", ex.ToString());
        }
    }

    private static async Task UpdateVideoMaps(Artcc artcc, TextBlock TextBlockLoading)
    {
        Logger.Info("vNas.UpdateVideoMaps", $"Checking: {artcc.id}");
        foreach (JObject videoMap in artcc.videoMaps)
        {
            string path = PathFinder.GetFilePath($"VideoMaps\\{artcc.id}", $"{videoMap["id"]}.geojson");
            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists || fileInfo.LastWriteTimeUtc < DateTime.Parse((string)videoMap["lastUpdatedAt"]))
            {
                Uri url = new Uri($"https://data-api.vnas.vatsim.net/Files/VideoMaps/{artcc.id}/{videoMap["id"]}.geojson");
                HttpResponseMessage resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (resp.StatusCode == HttpStatusCode.NotFound)
                {
                    File.Delete(PathFinder.GetFilePath($"VideoMaps\\{artcc.id}", $"{videoMap["id"]}.geojson"));
                    continue;
                }
                resp.EnsureSuccessStatusCode();
                string jsonText = await resp.Content.ReadAsStringAsync();
                JObject downloadedVideoMap = JsonConvert.DeserializeObject<JObject>(jsonText);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllTextAsync(path, downloadedVideoMap.ToString(Formatting.Indented));
                TextBlockLoading.Text = $"Updated VideoMap: {videoMap["id"]}";
                Logger.Info("vNas.UpdateVideoMaps", $"Updated: {videoMap["id"]}");
            }
        }
    }
}
