using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows.Input;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Services;
using vFalcon.Utils;
using Message = vFalcon.Utils.Message;

namespace vFalcon.UI.ViewModels.Common;

public class ManageArtccsViewModel : ViewModelBase
{
    private ArtccService artccService = new ArtccService();
    private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
    private Dictionary<string, int> artccProfileCount = new Dictionary<string, int>();
    private ObservableCollection<ArtccViewModel> installedArtccs = new ObservableCollection<ArtccViewModel>();

    public bool IsInstallingArtcc = false;
    public string IsInstallingArtccId = string.Empty;
    public ArtccViewModel? InstalledArtcc;
    public ConcurrentDictionary<string, CancellationTokenSource> CancellationTokenSources = new(StringComparer.OrdinalIgnoreCase);
    public ObservableCollection<ArtccViewModel> InstalledArtccs
    {
        get => installedArtccs;
        set
        {
            installedArtccs = value;
            OnPropertyChanged();
        }
    }

    public ICommand InstallUninstallCommand { get; set; }

    public ManageArtccsViewModel()
    {
        Initialize();
        InstallUninstallCommand = new RelayCommand<ArtccViewModel>(OnInstallUninstallCommand);
    }

    private async void Initialize()
    {
        await SetArtccProfileCount();
        foreach (string name in NasArtccNames.Get)
        {
            string artccId = GetArtccId(name);
            bool installed = File.Exists(PathFinder.GetFilePath("ARTCCs", $"{artccId}.json"));
            int profileCount = artccProfileCount?.GetValueOrDefault(artccId) ?? 0;
            InstalledArtccs.Add(new ArtccViewModel
            {
                Name = name,
                IsInstalled = installed,
                IsInstalling = false,
                InstallUninstallText = $"{(installed ? "Uninstall" : "Install")}",
                StatusTextForeground = $"{(installed ? "#476ded" : "#858585")}",
                ArtccStatusTextForeground = $"{(installed ? "#ffffff" : "#858585")}",
                StatusText = $"{(installed ? $"Installed, {profileCount} profiles" : "Not installed")}",
                Profiles = profileCount
            });
        }
    }

    private async Task SetArtccProfileCount()
    {
        string profilesPath = PathFinder.GetFolderPath("Profiles");
        var files = Directory.GetFiles(profilesPath, "*.json");
        foreach (var file in files)
        {
            var json = await File.ReadAllTextAsync(file);
            var profile = JsonConvert.DeserializeObject<Profile>(json);
            if (profile == null || string.IsNullOrWhiteSpace(profile.ArtccId)) continue;

            artccProfileCount.TryGetValue(profile.ArtccId, out var count);
            artccProfileCount[profile.ArtccId] = count + 1;
        }
    }

    private static string GetArtccId(string selectedArtcc)
    {
        if (string.IsNullOrWhiteSpace(selectedArtcc)) return string.Empty;
        int hyphenIndex = selectedArtcc.LastIndexOf('-');
        return hyphenIndex >= 0 ? selectedArtcc[(hyphenIndex + 1)..].Trim() : selectedArtcc.Trim();
    }

    private async void OnInstallUninstallCommand(ArtccViewModel item)
    {
        if (item == null) return;
        string artccId = GetArtccId(item.Name);
        string path = PathFinder.GetFilePath("ARTCCs", $"{GetArtccId(item.Name)}.json");

        if (item.IsInstalled)
        {
            if (item.Profiles > 0)
            {
                Message.Error("You cannot uninstall an ARTCC that is in use by one or more profiles");
                return;
            }
            if (File.Exists(path)) File.Delete(path);
            Directory.Delete(PathFinder.GetFolderPath($"VideoMaps\\{artccId}"), true);
            item.IsInstalled = false;
            item.InstallUninstallText = "Install";
            item.StatusTextForeground = "#858585";
            item.ArtccStatusTextForeground = "#858585";
            item.Profiles = 0;

            InstalledArtcc = null;
            IsInstallingArtccId = string.Empty;
            IsInstallingArtcc = false;
        }
        else if (item.IsInstalling)
        {
            if (CancellationTokenSources.TryRemove(artccId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
            InstalledArtcc = null;
            IsInstallingArtccId = string.Empty;
            IsInstallingArtcc = false;
            item.IsInstalling = false;
            item.InstallUninstallText = "Install";
            item.StatusText = "Not Installed";
            item.StatusTextForeground = "#858585";
            item.ArtccStatusTextForeground = "#858585";
            if (File.Exists(path)) File.Delete(path);
            try
            {
                Directory.Delete(PathFinder.GetFolderPath($"VideoMaps\\{artccId}"), true);
            }
            catch (Exception) { }
        }
        else if (!item.IsInstalled && !IsInstallingArtcc)
        {
            InstalledArtcc = item;
            IsInstallingArtccId = artccId;
            IsInstallingArtcc = true;
            item.IsInstalling = true;
            item.InstallUninstallText = "Cancel";

            var cts = new CancellationTokenSource();
            CancellationTokenSources[artccId] = cts;
            var ct = cts.Token;

            try
            {
                var metaUrl = $"https://data-api.vnas.vatsim.net/api/artccs/{artccId}";
                using var resp = await http.GetAsync(metaUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                resp.EnsureSuccessStatusCode();
                var jsonText = await resp.Content.ReadAsStringAsync(ct);

                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                await File.WriteAllTextAsync(path, jsonText, Encoding.UTF8, ct);

                var artcc = await artccService.GetArtcc(artccId);
                var geoMaps = (JArray)artcc.videoMaps;
                int totalFileCount = geoMaps.Count;

                int currentFile = 1;
                item.StatusTextForeground = "#32CD32";

                var artccSeg = Uri.EscapeDataString(artccId ?? string.Empty);

                foreach (JObject videoMap in artcc.videoMaps)
                {
                    item.StatusText = $"Downloading {currentFile} of {totalFileCount}";

                    var videoMapId = (string?)videoMap["id"] ?? string.Empty;
                    var mapSeg = Uri.EscapeDataString(videoMapId);
                    var fileUrl = new Uri($"https://data-api.vnas.vatsim.net/Files/VideoMaps/{artccSeg}/{mapSeg}.geojson");
                    var savePath = PathFinder.GetFilePath($"VideoMaps\\{artccId}", $"{videoMapId}.geojson");
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

                    var tmpPath = savePath + ".tmp";

                    try
                    {
                        using var r = await http.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                        if (!r.IsSuccessStatusCode)
                        {
                            var body = await r.Content.ReadAsStringAsync(ct);
                            Logger.Error("VideoMap", $"{fileUrl} -> {(int)r.StatusCode} {r.ReasonPhrase} | {body}");
                            continue;
                        }

                        await using var inStream = await r.Content.ReadAsStreamAsync(ct);
                        await using (var outStream = File.Create(tmpPath))
                            await inStream.CopyToAsync(outStream, ct);

                        File.Move(tmpPath, savePath, overwrite: true);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        Logger.Error("VideoMap", ex.ToString());
                        try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { }
                    }
                    finally
                    {
                        currentFile++;
                    }
                }

                InstalledArtcc = null;
                IsInstallingArtccId = string.Empty;
                IsInstallingArtcc = false;
                item.StatusText = $"Installed, {item.Profiles} profiles";
                item.IsInstalled = true;
                item.InstallUninstallText = "Uninstall";
                item.StatusTextForeground = "#476ded";
                item.ArtccStatusTextForeground = "#ffffff";
                item.Profiles = 0;
            }
            catch (OperationCanceledException)
            {
                item.StatusText = "Not installed";
                item.StatusTextForeground = "#858585";
                item.ArtccStatusTextForeground = "#858585";
                item.IsInstalled = false;
                item.InstallUninstallText = "Install";
            }
            finally
            {
                item.IsInstalling = false;
                if (CancellationTokenSources.TryRemove(artccId, out var existing))
                    existing.Dispose();
            }
        }
    }
}
