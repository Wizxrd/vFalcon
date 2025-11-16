using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vFalcon.Helpers
{
    public class YourClass
    {
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
            string expectedAsset = $"vFalcon-{tag}.zip";

            string assetUrl = "";
            if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
            {
                foreach (var a in assets.EnumerateArray())
                {
                    var name = a.GetProperty("name").GetString() ?? "";
                    var url = a.GetProperty("browser_download_url").GetString() ?? "";
                    if (string.Equals(name, expectedAsset, StringComparison.OrdinalIgnoreCase))
                    {
                        assetUrl = url;
                        break;
                    }
                }
            }

            return (tag, expectedAsset, assetUrl);
        }

        private async Task<string> DownloadAssetAsync(string url, string tag)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "vFalconUpdate", tag);
            Directory.CreateDirectory(tempDir);
            var zipPath = Path.Combine(tempDir, $"vFalcon-{tag}.zip");

            using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();
            await using (var fs = File.Create(zipPath))
            {
                await resp.Content.CopyToAsync(fs);
            }

            return zipPath;
        }

        private string ExtractZip(string zipPath)
        {
            var extractDir = Path.Combine(Path.GetDirectoryName(zipPath)!, "extracted");
            if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
            ZipFile.ExtractToDirectory(zipPath, extractDir);
            return extractDir;
        }

        private void CopyFilesIntoApp(string sourceDir)
        {
            var appDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName ?? "");

            foreach (var srcPath in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(sourceDir, srcPath);
                var destPath = Path.Combine(appDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);

                if (string.Equals(Path.GetFileName(destPath), exeName, StringComparison.OrdinalIgnoreCase))
                    continue;

                File.Copy(srcPath, destPath, true);
            }
        }

        public async Task CheckForUpdate()
        {
            var current = GetCurrentVersion();
            var (tag, assetName, assetUrl) = await GetLatestReleaseAsync();

            if (!TryParseTagToVersion(tag, out var latest)) return;
            if (latest <= current) return;
            if (string.IsNullOrWhiteSpace(assetUrl)) return;

            var zipPath = await DownloadAssetAsync(assetUrl, tag);
            var extracted = ExtractZip(zipPath);
            var payloadRoot = Directory.Exists(Path.Combine(extracted, "vFalcon")) ? Path.Combine(extracted, "vFalcon") : extracted;
            //CopyFilesIntoApp(payloadRoot);
        }
    }
}
