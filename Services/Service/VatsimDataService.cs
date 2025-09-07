using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Threading;
using vFalcon.DataFeeds;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Service;
using vFalcon.ViewModels;

public class VatsimDataService
{
    private readonly DispatcherTimer refreshTimer = new();
    private readonly PilotService pilotService;
    private readonly Profile profile;
    private readonly Action invalidateCanvas;

    // Store the last update timestamp to avoid redundant updates
    private DateTime? lastUpdateTimestamp = null;

    public VatsimDataService(PilotService pilotService, Profile profile, Action invalidateCanvas)
    {
        this.pilotService = pilotService;
        this.profile = profile;
        this.invalidateCanvas = invalidateCanvas;
    }

    public async void Start()
    {
        refreshTimer.Interval = TimeSpan.FromSeconds(15); // Initial interval
        refreshTimer.Tick += async (s, e) => await RefreshAsync();
        refreshTimer.Start();
        await RefreshAsync();
    }

    public async void Refresh()
    {
        await RefreshAsync();
    }

    public void Stop()
    {
        refreshTimer.Stop();
    }
    private async Task RefreshAsync(CancellationToken ct = default)
    {
        try
        {
            var dataFeed = await VatsimDataFeed.GetPilotsAsync(ct);
            if (dataFeed is null) return;

            string timestampStr = dataFeed["general"]?["update_timestamp"]?.ToString();
            DateTime lastUpdateUtc;

            if (DateTime.TryParse(timestampStr, out lastUpdateUtc))
            {
                lastUpdateUtc = lastUpdateUtc.AddMilliseconds(-lastUpdateUtc.Millisecond);
                Logger.Debug("VatsimDataService.RefreshAsync", $"Last update timestamp: {lastUpdateUtc}");

                if (lastUpdateTimestamp.HasValue && lastUpdateTimestamp.Value == lastUpdateUtc)
                {
                    Logger.Debug("VatsimDataService.RefreshAsync", "Skipping refresh, update timestamp is the same.");
                    return;
                }

                lastUpdateTimestamp = lastUpdateUtc;
            }
            var transceiverFrequencies = await VatsimDataFeed.GetTransceiversAsync(ct);
            var sectorFreq = await vNasDataFeed.GetArtccFrequencyAsync(profile.ArtccId, profile.ActivatedSectorName, ct);

            // If CPU-bound:
            await Task.Run(() => pilotService.UpdateFromDataFeed(dataFeed, transceiverFrequencies, sectorFreq), ct);

            // Back to UI:
            Application.Current.Dispatcher.BeginInvoke(invalidateCanvas);
        }
        catch (OperationCanceledException) { /* ignore */ }
        catch (Exception ex)
        {
            Logger.Error("VatsimDataService.RefreshAsync", ex.ToString());
        }
    }

}

