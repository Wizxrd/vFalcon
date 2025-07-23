using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using vFalcon.DataFeed;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Services
{
    public class VatsimDataService
    {
        private readonly DispatcherTimer refreshTimer = new();
        private readonly PilotService pilotService;
        private readonly Profile profile;
        private readonly ArtccBox artccBox;
        private readonly Action invalidateCanvas;

        public VatsimDataService(PilotService pilotService, Profile profile, ArtccBox artccBox, Action invalidateCanvas)
        {
            this.pilotService = pilotService;
            this.profile = profile;
            this.artccBox = artccBox;
            this.invalidateCanvas = invalidateCanvas;
        }

        public async void Start()
        {
            refreshTimer.Interval = TimeSpan.FromSeconds(30);
            refreshTimer.Tick += async (s, e) => await RefreshAsync();
            refreshTimer.Start();
            await RefreshAsync();
        }

        public async void Refresh()
        {
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            Logger.Debug("RefreshAsync", "Refreshing");

            try
            {
                JObject? dataFeed = await VatsimDataFeed.AsyncGet();
                if (dataFeed == null) return;

                var transceiverFrequencies = await VatsimApiService.LoadTransceiverFrequenciesAsync();
                string? sectorFreq = await VatsimApiService.GetArtccFrequencyAsync(profile.ArtccId, profile.LastSectorName);

                await Task.Run(() =>
                {
                    pilotService.UpdateFromDataFeed(dataFeed, transceiverFrequencies, sectorFreq, artccBox);
                });

                invalidateCanvas();
            }
            catch (Exception ex)
            {
                Logger.Error("VatsimDataService.RefreshAsync", ex.ToString());
            }
        }
    }
}
