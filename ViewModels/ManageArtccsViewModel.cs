using AdonisUI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Renderers;
using vFalcon.Services.Service;

namespace vFalcon.ViewModels
{
    public class ManageArtccsViewModel : ViewModelBase
    {
        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        private Dictionary<string, int> ArtccProfileCount = new Dictionary<string, int>();

        public class ArtccItem : ViewModelBase
        {
            string name = string.Empty;
            string statusText = string.Empty;
            bool isInstalled;
            int profiles;
            string installUninstallText = string.Empty;
            string statusTextForeground = string.Empty;
            string artccStatusTextForeground = string.Empty;
            bool isInstalling;

            public string ArtccStatusTextForeground
            {
                get => artccStatusTextForeground;
                set
                {
                    artccStatusTextForeground = value;
                    OnPropertyChanged();
                }
            }

            public string StatusTextForeground
            {
                get => statusTextForeground;
                set
                {
                    statusTextForeground = value;
                    OnPropertyChanged();
                }
            }

            public string InstallUninstallText
            {
                get => installUninstallText;
                set
                {
                    installUninstallText = value;
                    OnPropertyChanged();
                }
            }
            public bool IsInstalling
            {
                get => isInstalling;
                set
                {
                    isInstalling = value;
                    OnPropertyChanged();
                }
            }
            public string Name { get => name; set { if (value == name) return; name = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); } }
            public bool IsInstalled { get => isInstalled; set { if (value == isInstalled) return; isInstalled = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); } }
            public int Profiles { get => profiles; set { if (value == profiles) return; profiles = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); } }
            public string StatusText
            {
                get => statusText;
                set
                {
                    statusText = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<ArtccItem> artccItems = new ObservableCollection<ArtccItem>();
        public ObservableCollection<ArtccItem> ArtccItems
        {
            get => artccItems;
            set
            {
                artccItems = value; 
                OnPropertyChanged();
            }
        }
        public ICommand InstallUninstallCommand { get; }

        private async Task SetArtccProfileCount()
        {
            string profilesPath = Loader.LoadFolder("Profiles");
            var files = Directory.GetFiles(profilesPath, "*.json");
            foreach (var file in files)
            {
                var json = await File.ReadAllTextAsync(file);
                var profile = JsonConvert.DeserializeObject<Profile>(json);
                if (profile == null || string.IsNullOrWhiteSpace(profile.ArtccId)) continue;

                ArtccProfileCount.TryGetValue(profile.ArtccId, out var count);
                ArtccProfileCount[profile.ArtccId] = count + 1;
            }
        }

        public static string GetArtccId(string selectedArtcc)
        {
            if (string.IsNullOrWhiteSpace(selectedArtcc)) return string.Empty;
            int hyphenIndex = selectedArtcc.LastIndexOf('-');
            return hyphenIndex >= 0 ? selectedArtcc[(hyphenIndex + 1)..].Trim() : selectedArtcc.Trim();
        }
        public ManageArtccsViewModel()
        {
            Initialize();
            InstallUninstallCommand = new RelayCommand<ArtccItem>(OnInstallUninstall);
        }

        private async void Initialize()
        {
            await SetArtccProfileCount();
            foreach (string name in ArtccList.ArtccNames)
            {
                string artccId = GetArtccId(name);
                bool installed = File.Exists(Loader.LoadFile("ARTCCs", $"{artccId}.json"));
                int profileCount = ArtccProfileCount?.GetValueOrDefault(artccId) ?? 0;
                ArtccItems.Add(new ArtccItem
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
        public static void ShowErrorDialog(Window owner, string message, string? details = null)
        {
            var model = new MessageBoxModel
            {
                Caption = "Error",
                Text = string.IsNullOrWhiteSpace(details) ? message : $"{message}\n\nDetails:\n{details}",
                Icon = AdonisUI.Controls.MessageBoxImage.Error,
                Buttons = new[]
                {
                    MessageBoxButtons.Ok("OK")
                },
            };

            AdonisUI.Controls.MessageBox.Show(owner, model);
        }

        private ArtccService artccService = new ArtccService();
        public ConcurrentDictionary<string, CancellationTokenSource> _installCts = new(StringComparer.OrdinalIgnoreCase);
        public bool IsInstallingArtcc = false;
        public string IsInstallingArtccId = string.Empty;
        public ArtccItem? InstallingArtccItem;
        private async void OnInstallUninstall(ArtccItem item)
        {
            if (item == null) return;
            string artccId = GetArtccId(item.Name);
            string path = Loader.LoadFile("ARTCCs", $"{GetArtccId(item.Name)}.json");

            if (item.IsInstalled)
            {
                if (item.Profiles > 0)
                {
                    ShowErrorDialog(Application.Current.Windows[0], "You cannot uninstall an ARTCC that is in use by one or more profiles");
                    return;
                }
                if (File.Exists(path)) File.Delete(path);
                Directory.Delete(Loader.LoadFolder($"VideoMaps\\{artccId}"), true);
                item.IsInstalled = false;
                item.InstallUninstallText = "Install";
                item.StatusTextForeground = "#858585";
                item.ArtccStatusTextForeground = "#858585";
                item.Profiles = 0;

                InstallingArtccItem = null;
                IsInstallingArtccId = string.Empty;
                IsInstallingArtcc = false;
            }
            else if (item.IsInstalling)
            {
                if (_installCts.TryRemove(artccId, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                InstallingArtccItem = null;
                IsInstallingArtccId = string.Empty;
                IsInstallingArtcc = false;
                item.IsInstalling = false;
                item.InstallUninstallText = "Install";
                item.StatusText = "Not Installed";
                item.StatusTextForeground = "#858585";
                item.ArtccStatusTextForeground = "#858585";
                if (File.Exists(path)) File.Delete(path);
                Directory.Delete(Loader.LoadFolder($"VideoMaps\\{artccId}"), true);
            }
            else if (!item.IsInstalled && !IsInstallingArtcc)
            {
                InstallingArtccItem = item;
                IsInstallingArtccId = artccId;
                IsInstallingArtcc = true;
                item.IsInstalling = true;
                item.InstallUninstallText = "Cancel";

                var cts = new CancellationTokenSource();
                _installCts[artccId] = cts;
                var ct = cts.Token;

                try
                {
                    var metaUrl = $"https://data-api.vnas.vatsim.net/api/artccs/{artccId}";
                    using var resp = await http.GetAsync(metaUrl, HttpCompletionOption.ResponseHeadersRead, ct);
                    resp.EnsureSuccessStatusCode();
                    var jsonText = await resp.Content.ReadAsStringAsync(ct);

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                    await File.WriteAllTextAsync(path, jsonText, Encoding.UTF8, ct);

                    var artcc = await artccService.LoadArtcc(artccId);
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
                        Logger.Debug("Downloading", fileUrl.ToString());
                        var savePath = Loader.LoadFile($"VideoMaps\\{artccId}", $"{videoMapId}.geojson");
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

                    InstallingArtccItem = null;
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
                    if (_installCts.TryRemove(artccId, out var existing))
                        existing.Dispose();
                }
            }
        }
    }
}
