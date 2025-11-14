using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Controls;
using vFalcon.Nasr.Models;
using vFalcon.Nasr.Parsers;
using vFalcon.Utils;

namespace vFalcon.Nasr;

public class NavData
{
    private readonly Uri BasePrefix = new("https://nfdc.faa.gov/webContent/28DaySub/", UriKind.Absolute);
    private const string FilePrefix = "28DaySubscription_Effective_";
    private const string FileSuffix = ".zip";

    public TextBlock TextBlockLoading { get; set; } = new TextBlock();
    public bool ForceDownload { get; set; }
    public int Delay { get; set; }
    public bool SwapNavDate { get; set; }

    public string Date { get; set; }

    private static readonly string root = PathFinder.GetAppDirectory();
    private static string statePath = string.Empty;
    private static string downloadDir = string.Empty;
    private static string extractDir = string.Empty;

    private static readonly string appDirectory = PathFinder.GetAppDirectory();
    private static readonly string userSelectedSourceDirectory = $"{appDirectory}\\Nasr\\CsvData";
    private static readonly string userSelectedOutputDirectory = $"{appDirectory}\\NavData";

    private record CycleState
    {
        public DateTime Date { get; init; }
        public int Cycle { get; init; }
    }

    public NavData(string? date)
    {
        if (string.IsNullOrEmpty(date))
        {
            var table = GetAiracTable();
            var latest = table.Last(t => t.Date <= DateTime.UtcNow.Date);
            Date = latest.Date.ToString("yyyy-MM-dd");
        }
        else
        {
            Date = date;
        }
    }

    public async Task Run()
    {
        statePath = Path.Combine($"{root}", "NavDataSerial.json");
        downloadDir = Path.Combine($"{root}\\Nasr", "Downloads");
        extractDir = Path.Combine($"{root}\\Nasr", "CsvData");
        Directory.CreateDirectory(downloadDir);
        Directory.CreateDirectory(extractDir);
        await CheckAndDownloadAsync();
    }

    public async Task DownloadParserData()
    {
        try
        {
            Directory.CreateDirectory($"{userSelectedOutputDirectory}\\{Date}");
            TextBlockLoading.Text = "Creating nasr csv files";
            await Task.Delay(Delay);
            GenerateAirportCsv();
            await Task.Delay(Delay);
            GenerateAwyCsv();
            await Task.Delay(Delay);
            GenerateFixCsv();
            await Task.Delay(Delay);
            GenerateDpCsv();
            await Task.Delay(Delay);
            GenerateNavAidCsv();
            await Task.Delay(Delay);
            GenerateStarCsv();
            await Task.Delay(Delay);
            Directory.Delete(downloadDir, recursive: true);
            Directory.Delete(extractDir, recursive: true);
#if !DEBUG
            Directory.Delete($"{PathFinder.GetAppDirectory()}\\Nasr", recursive: true);
#endif
        }
        catch (Exception ex)
        {
            Logger.Error("DownloadParserData", ex.ToString());
        }
    }

    public async Task CheckAndDownloadAsync(CancellationToken ct = default)
    {
        TextBlockLoading.Text = "Verifying airac cycle";
        await Task.Delay(Delay);
        var table = GetAiracTable();
        if (table.Count == 0) return;
        if (Directory.Exists(Path.Combine(userSelectedOutputDirectory, Date))) return;
        else ForceDownload = true;
        var latest = table.Last(t => DateTime.Parse(Date) == t.Date);
        if (ForceDownload)
        {
            TextBlockLoading.Text = "Forced nav data update";
            await Task.Delay(Delay);
            await DownloadAndExtractCycleAsync(latest.Date, ct, forceHttpDownload: true);
            await SaveAsync(new CycleState { Date = latest.Date, Cycle = latest.Cycle }, ct);
            await DownloadParserData();
            return;
        }

        var state = await LoadAsync(ct);
        if (state is null)
        {
            TextBlockLoading.Text = "NavDataSerial.json null";
            await Task.Delay(Delay);
            await DownloadAndExtractCycleAsync(latest.Date, ct);
            await SaveAsync(new CycleState { Date = latest.Date, Cycle = latest.Cycle }, ct);
            await DownloadParserData();
            return;
        }

        if (latest.Date <= state.Date) return;

        foreach (var entry in table.Where(t => t.Date > state.Date && t.Date <= latest.Date))
        {
            TextBlockLoading.Text = "Airac outdated";
            await Task.Delay(Delay);
            await DownloadAndExtractCycleAsync(entry.Date, ct);
            if (!SwapNavDate)
            {
                await SaveAsync(new CycleState { Date = entry.Date, Cycle = entry.Cycle }, ct);
            }
            await DownloadParserData();
        }
    }

    private void GenerateAirportCsv()
    {
        TextBlockLoading.Text = "Creating aiport csv";
        AptCsvParser aptCsvParser = new AptCsvParser();
        AptCsvDataCollection allParsedAptData = new AptCsvDataCollection();
        allParsedAptData.AptBase = aptCsvParser.ParseAptBase(Path.Combine(userSelectedSourceDirectory, "APT_BASE.csv")).AptBase;
        var csvPath = Path.Combine($"{userSelectedOutputDirectory}\\{Date}", "Airports.csv");
        using var sw = new StreamWriter(csvPath, false, new UTF8Encoding(false));
        var ci = CultureInfo.InvariantCulture;
        sw.WriteLine("ArptId,Lat,Lon");
        foreach (var rec in allParsedAptData.AptBase)
        {
            sw.Write(rec.ArptId);
            sw.Write(',');
            sw.Write(rec.BaseLatDecimal);
            sw.Write(',');
            sw.WriteLine(rec.BaseLongDecimal);
        }
    }

    private void GenerateAwyCsv()
    {
        TextBlockLoading.Text = "Creating airway csv";
        AwyCsvParser awyCsvParser = new AwyCsvParser();
        AwyCsvDataCollection allParsedAwyData = new AwyCsvDataCollection();
        allParsedAwyData.AwyBase = awyCsvParser.ParseAwyBase(Path.Combine(userSelectedSourceDirectory, "AWY_BASE.csv")).AwyBase;
        var csvPath = Path.Combine($"{userSelectedOutputDirectory}\\{Date}", "Airways.csv");
        using var sw = new StreamWriter(csvPath, false, new UTF8Encoding(false));
        var ci = CultureInfo.InvariantCulture;
        sw.WriteLine("AwyId,AirwayString");
        foreach (var rec in allParsedAwyData.AwyBase)
        {
            sw.Write(rec.AwyId);
            sw.Write(',');
            sw.WriteLine(rec.AirwayString);
        }
    }

    private void GenerateDpCsv()
    {
        TextBlockLoading.Text = "Creating departure csv";
        DpCsvParser dpCsvParser = new DpCsvParser();
        DpCsvDataCollection allParsedDpData = new DpCsvDataCollection();
        allParsedDpData.DpRte = dpCsvParser.ParseDpRte(Path.Combine(userSelectedSourceDirectory, "DP_RTE.csv")).DpRte;
        var csvPath = Path.Combine($"{userSelectedOutputDirectory}\\{Date}", "Departures.csv");
        using var sw = new StreamWriter(csvPath, false, new UTF8Encoding(false));
        var ci = CultureInfo.InvariantCulture;
        sw.WriteLine("TransitionComputerCode,RouteString");

        var groups = allParsedDpData.DpRte
            .Where(r => r.RoutePortionType != "BODY")
            .GroupBy(r => new { r.TransitionComputerCode, r.RoutePortionType });

        foreach (var g in groups)
        {
            var ordered = g.OrderBy(r => r.PointSeq);
            var parts = new List<string>();

            foreach (var r in ordered)
            {
                if (parts.Count == 0 && !string.IsNullOrWhiteSpace(r.Point))
                    parts.Add(r.Point.Trim());

                if (!string.IsNullOrWhiteSpace(r.NextPoint))
                {
                    var nxt = r.NextPoint.Trim();
                    if (parts.Count == 0 || !parts[^1].Equals(nxt, StringComparison.OrdinalIgnoreCase))
                        parts.Add(nxt);
                }
            }

            parts.Reverse();
            var routeString = string.Join(' ', parts);

            sw.Write(g.Key.TransitionComputerCode.Replace(".", " "));
            sw.Write(',');
            sw.WriteLine(CsvEscape(routeString));
        }
    }

    private void GenerateFixCsv()
    {
        TextBlockLoading.Text = "Creating fix csv";
        FixCsvParser fixCsvParser = new FixCsvParser();
        FixCsvDataCollection allParsedFixData = new FixCsvDataCollection();
        allParsedFixData.FixBase = fixCsvParser.ParseFixBase(Path.Combine(userSelectedSourceDirectory, "FIX_BASE.csv")).FixBase;
        var csvPath = Path.Combine($"{userSelectedOutputDirectory}\\{Date}", "Fixes.csv");
        using var sw = new StreamWriter(csvPath, false, new UTF8Encoding(false));
        var ci = CultureInfo.InvariantCulture;
        sw.WriteLine("FixId,Lat,Lon");
        foreach (var rec in allParsedFixData.FixBase)
        {
            sw.Write(rec.FixId);
            sw.Write(',');
            sw.Write(rec.LatDecimal);
            sw.Write(',');
            sw.WriteLine(rec.LongDecimal);
        }
    }

    private void GenerateNavAidCsv()
    {
        TextBlockLoading.Text = "Creating navAid csv";
        NavCsvParser navCsvParser = new NavCsvParser();
        NavCsvDataCollection allParsedNavData = new NavCsvDataCollection();
        allParsedNavData.NavBase = navCsvParser.ParseNavBase(Path.Combine(userSelectedSourceDirectory, "NAV_BASE.csv")).NavBase;
        var csvPath = Path.Combine($"{userSelectedOutputDirectory}\\{Date}", "NavAids.csv");
        using var sw = new StreamWriter(csvPath, false, new UTF8Encoding(false));
        var ci = CultureInfo.InvariantCulture;
        sw.WriteLine("NavId,Lat,Lon");
        foreach (var rec in allParsedNavData.NavBase)
        {
            sw.Write(rec.NavId);
            sw.Write(',');
            sw.Write(rec.LatDecimal);
            sw.Write(',');
            sw.WriteLine(rec.LongDecimal);
        }
    }

    private void GenerateStarCsv()
    {
        TextBlockLoading.Text = "Creating star csv";
        StarCsvParser starCsvParser = new StarCsvParser();
        StarCsvDataCollection allParsedStarData = new StarCsvDataCollection();
        allParsedStarData.StarRte = starCsvParser.ParseStarRte(Path.Combine(userSelectedSourceDirectory, "STAR_RTE.csv")).StarRte;
        var csvPath = Path.Combine($"{userSelectedOutputDirectory}\\{Date}", "Stars.csv");
        using var sw = new StreamWriter(csvPath, false, new UTF8Encoding(false));
        var ci = CultureInfo.InvariantCulture;
        sw.WriteLine("TransitionComputerCode,RouteString");

        var groups = allParsedStarData.StarRte
            .Where(r => r.RoutePortionType == "TRANSITION")
            .GroupBy(r => new { r.TransitionComputerCode, r.RoutePortionType });

        foreach (var g in groups)
        {
            var ordered = g.OrderBy(r => r.PointSeq);
            var parts = new List<string>();

            foreach (var r in ordered)
            {
                if (parts.Count == 0 && !string.IsNullOrWhiteSpace(r.Point))
                    parts.Add(r.Point.Trim());

                if (!string.IsNullOrWhiteSpace(r.NextPoint))
                {
                    var nxt = r.NextPoint.Trim();
                    if (parts.Count == 0 || !parts[^1].Equals(nxt, StringComparison.OrdinalIgnoreCase))
                        parts.Add(nxt);
                }
            }

            parts.Reverse();
            var routeString = string.Join(' ', parts);

            sw.Write(g.Key.TransitionComputerCode.Replace(".", " "));
            sw.Write(',');
            sw.WriteLine(CsvEscape(routeString));
        }
    }

    private static List<(DateTime Date, int Cycle)> GetAiracTable()
    {
        var list = new List<(DateTime Date, int Cycle)>(AiracCycleDates.DateCycle.Count);
        foreach (var kvp in AiracCycleDates.DateCycle)
        {
            if (DateTime.TryParseExact(kvp.Key, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var d)
                && int.TryParse(kvp.Value, out var c))
            {
                list.Add((d.Date, Cycle: c));
            }
        }

        list.Sort((a, b) => a.Date.CompareTo(b.Date));
        return list;
    }
    private async Task DownloadAndExtractCycleAsync(DateTime effectiveUtc, CancellationToken ct, bool forceHttpDownload = false)
    {
        void Update(string msg)
        {
            TextBlockLoading.Text = msg;
        }

        string yyyyMmDd = effectiveUtc.ToString("yyyy-MM-dd");
        string fileName = $"{FilePrefix}{yyyyMmDd}{FileSuffix}";
        var url = new Uri(BasePrefix, fileName);
        string destZip = Path.Combine(downloadDir, fileName);
        string workDir = Path.Combine(downloadDir, yyyyMmDd);
        string finalDir = extractDir;

        if (forceHttpDownload && File.Exists(destZip))
        {
            try { File.Delete(destZip); } catch { /* ignore */ }
        }

        if (!File.Exists(destZip) || new FileInfo(destZip).Length == 0)
        {
            Update("Requesting nasr package");
            using var http = new HttpClient();
            using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();

            var contentLength = resp.Content.Headers.ContentLength ?? -1L;
            var tmp = destZip + ".partial";

            Update("Downloading from nasr: 0%");

            await using (var input = await resp.Content.ReadAsStreamAsync(ct))
            await using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                var buffer = new byte[81920];
                long totalRead = 0;
                int read;
                int lastPct = -1;
                var sw = Stopwatch.StartNew();

                while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
                {
                    await fs.WriteAsync(buffer.AsMemory(0, read), ct);
                    totalRead += read;

                    if (contentLength > 0)
                    {
                        // throttle UI updates (~5/sec) and only when pct changes
                        int pct = (int)(totalRead * 100L / contentLength);
                        if (pct != lastPct && sw.ElapsedMilliseconds >= 200)
                        {
                            Update($"Downloading from nasr: {pct}%");
                            lastPct = pct;
                            sw.Restart();
                        }
                    }
                }
            }

            if (File.Exists(destZip)) File.Delete(destZip);
            File.Move(tmp, destZip);
            Update("Download complete");
        }
        else
        {
            Update("Using cached nasr package.");
        }

        // Extract outer zip
        Update("Extracting package");
        if (Directory.Exists(workDir)) Directory.Delete(workDir, true);
        Directory.CreateDirectory(workDir);
        ZipFile.ExtractToDirectory(destZip, workDir, true);
        try { File.Delete(destZip); } catch { }

        // Extract inner zips with progress
        var innerZips = Directory.EnumerateFiles(workDir, "*.zip", SearchOption.AllDirectories).ToList();
        if (innerZips.Count > 0)
        {
            for (int i = 0; i < innerZips.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var inner = innerZips[i];
                Update($"Extracting inner archives ({i + 1}/{innerZips.Count})...");
                var target = Path.GetDirectoryName(inner)!;
                ZipFile.ExtractToDirectory(inner, target, true);
                try { File.Delete(inner); } catch { }
            }
        }

        Update("Finalizing csv data");
        var csvDir = Directory.EnumerateDirectories(workDir, "CSV_Data", SearchOption.AllDirectories).FirstOrDefault();
        if (csvDir is null) throw new DirectoryNotFoundException("CSV_Data not found in NASR package.");

        if (Directory.Exists(finalDir)) Directory.Delete(finalDir, true);
        Directory.CreateDirectory(Path.GetDirectoryName(finalDir)!);
        MoveDirectoryReplace(csvDir, finalDir);
        Update("Cleaning up");
        try { Directory.Delete(workDir, true); } catch { }
        Update("NASR data ready");
    }

    private static void MoveDirectoryReplace(string sourceDir, string destDir)
    {
        try
        {
            Directory.Move(sourceDir, destDir);
        }
        catch
        {
            if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
            Directory.CreateDirectory(destDir);
            foreach (var dir in Directory.EnumerateDirectories(sourceDir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(sourceDir, dir);
                Directory.CreateDirectory(Path.Combine(destDir, rel));
            }
            foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(sourceDir, file);
                var to = Path.Combine(destDir, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(to)!);
                File.Copy(file, to, true);
            }
            try { Directory.Delete(sourceDir, true); } catch { }
        }
    }

    private async Task<CycleState?> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(statePath)) return null;
        await using var fs = new FileStream(statePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var doc = await JsonDocument.ParseAsync(fs, cancellationToken: ct);
        var rootEl = doc.RootElement;
        var dateStr = rootEl.GetProperty("Date").GetString();
        var cycleVal = rootEl.GetProperty("Cycle").GetInt32();
        if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            return null;
        return new CycleState { Date = d, Cycle = cycleVal };
    }

    private async Task SaveAsync(CycleState state, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(statePath)!);
        await using var fs = new FileStream(statePath, FileMode.Create, FileAccess.Write, FileShare.None);
        var payload = new { Date = state.Date.ToString("yyyy-MM-dd"), state.Cycle };
        await JsonSerializer.SerializeAsync(fs, payload, new JsonSerializerOptions { WriteIndented = true }, ct);
    }

    static string CsvEscape(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        bool mustQuote = s.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        return mustQuote ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
    }
}
