using AdonisUI.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Views
{
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
            await CheckForUpdate();
            TextBlockLoading.Text = "Importing Artccs";
            await ImportArtccs();
            TextBlockLoading.Text = "Importing Profiles";
            await ImportEramProfiles();
            Logger.Info("LoadingView.InitializeImports", "Completed");
            LoadProfileView loadProfileView = new LoadProfileView();
            this.Close();
            loadProfileView.ShowDialog();
        }

        private static readonly HttpClient http = new HttpClient();

        private Version GetCurrentVersion()
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrWhiteSpace(info))
            {
                var m = Regex.Match(info, @"(?<=^|[^0-9])(\d+\.\d+\.\d+(\.\d+)?)");
                if (m.Success && Version.TryParse(m.Value, out var v1)) return v1;
            }
            var v = asm.GetName().Version;
            return v ?? new Version(0, 0, 0, 0);
        }

        private static bool TryParseTagToVersion(string tag, out Version v)
        {
            v = new Version(0, 0, 0, 0);
            if (string.IsNullOrWhiteSpace(tag)) return false;
            var cleaned = tag.Trim();
            if (cleaned.StartsWith("v", StringComparison.OrdinalIgnoreCase)) cleaned = cleaned[1..];
            cleaned = Regex.Replace(cleaned, @"[-+].*$", "");
            return Version.TryParse(cleaned, out v);
        }

        private async Task<(string tag, string assetName, string assetUrl)> GetLatestReleaseAsync()
        {
            http.DefaultRequestHeaders.UserAgent.Clear();
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("vFalconUpdater", "1.0"));
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

            using var resp = await http.GetAsync("https://api.github.com/repos/Wizxrd/vFalcon/releases/latest");
            resp.EnsureSuccessStatusCode();
            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var root = doc.RootElement;
            var tag = root.GetProperty("tag_name").GetString() ?? "";
            var version = tag.StartsWith("v", StringComparison.OrdinalIgnoreCase) ? tag[1..] : tag;

            string assetName = "";
            string assetUrl = "";

            if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
            {
                var exact1 = $"vFalcon-{tag}.zip";
                var exact2 = $"vFalcon-{version}.zip";

                foreach (var a in assets.EnumerateArray())
                {
                    var name = a.GetProperty("name").GetString() ?? "";
                    var url = a.GetProperty("browser_download_url").GetString() ?? "";
                    if (string.Equals(name, exact1, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, exact2, StringComparison.OrdinalIgnoreCase))
                    {
                        assetName = name;
                        assetUrl = url;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(assetUrl))
                {
                    var rx = new Regex($@"^vFalcon-(?:v)?{Regex.Escape(version)}\.zip$", RegexOptions.IgnoreCase);
                    foreach (var a in assets.EnumerateArray())
                    {
                        var name = a.GetProperty("name").GetString() ?? "";
                        var url = a.GetProperty("browser_download_url").GetString() ?? "";
                        if (rx.IsMatch(name))
                        {
                            assetName = name;
                            assetUrl = url;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(assetUrl))
                {
                    foreach (var a in assets.EnumerateArray())
                    {
                        var name = a.GetProperty("name").GetString() ?? "";
                        var url = a.GetProperty("browser_download_url").GetString() ?? "";
                        if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                        {
                            assetName = name;
                            assetUrl = url;
                            break;
                        }
                    }
                }
            }

            return (tag, assetName, assetUrl);
        }

        private async Task<string> DownloadAssetAsync(string url, string tagOrVersion, string assetName)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "vFalconUpdate", tagOrVersion);
            Directory.CreateDirectory(tempDir);
            var zipPath = Path.Combine(tempDir, assetName);

            using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();
            await using var fs = File.Create(zipPath);
            await resp.Content.CopyToAsync(fs);

            return zipPath;
        }

        private string ExtractZip(string zipPath)
        {
            var extractDir = Path.Combine(Path.GetDirectoryName(zipPath)!, "extracted");
            if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
            ZipFile.ExtractToDirectory(zipPath, extractDir);
            return extractDir;
        }

        private static bool NeedsElevation(string path)
        {
            var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            return path.StartsWith(pf, StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith(pf86, StringComparison.OrdinalIgnoreCase);
        }

        private void RestartWithUpdater(string payloadRoot)
        {
            string appExe = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            string appDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            int pid = Process.GetCurrentProcess().Id;

            string updaterDir = Path.Combine(Path.GetTempPath(), "vFalconUpdateRunner");
            Directory.CreateDirectory(updaterDir);
            string scriptPath = Path.Combine(updaterDir, "update-and-restart.ps1");

            var sb = new StringBuilder();
            sb.AppendLine("$ErrorActionPreference = 'Stop'");
            sb.AppendLine($"$pidToWait = {pid}");
            sb.AppendLine($"$src = '{payloadRoot.Replace("'", "''")}'");
            sb.AppendLine($"$dst = '{appDir.Replace("'", "''")}'");
            sb.AppendLine($"$exe = '{appExe.Replace("'", "''")}'");
            sb.AppendLine("try { Wait-Process -Id $pidToWait -ErrorAction SilentlyContinue } catch {}");
            sb.AppendLine("Start-Sleep -Milliseconds 300");
            sb.AppendLine(@"robocopy ""$src"" ""$dst"" /E /R:2 /W:1 /NFL /NDL /NJH /NJS /NP > $null");
            sb.AppendLine(@"Start-Process -FilePath ""$exe""");
            sb.AppendLine(@"try { Remove-Item -LiteralPath ""$src"" -Recurse -Force -ErrorAction SilentlyContinue } catch {}");
            File.WriteAllText(scriptPath, sb.ToString(), Encoding.UTF8);

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            if (NeedsElevation(appDir)) psi.Verb = "runas";

            Process.Start(psi);

            Application.Current?.Shutdown();
            Environment.Exit(0);
        }

        public async Task CheckForUpdate()
        {
            #if !DEBUG
            TextBlockLoading.Text = "Checking For Update";
            var current = GetCurrentVersion();
            TextBlockLoading.Text = $"Current Version: {current}";
            var (tag, assetName, assetUrl) = await GetLatestReleaseAsync();
            if (!TryParseTagToVersion(tag, out var latest)) return;
            if (latest <= current)
            {
                TextBlockLoading.Text = "vFalcon up-to-date";
                return;
            }
            if (string.IsNullOrWhiteSpace(assetUrl)) return;
            TextBlockLoading.Text = $"New Version: {latest}";
            var zipPath = await DownloadAssetAsync(assetUrl, tag, assetName);
            TextBlockLoading.Text = $"Downloaded: {zipPath}";
            var extracted = ExtractZip(zipPath);
            var payloadRoot = Directory.Exists(Path.Combine(extracted, "vFalcon")) ? Path.Combine(extracted, "vFalcon") : extracted;
            TextBlockLoading.Text = "Updating and restarting";
            RestartWithUpdater(payloadRoot);
            #endif
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
