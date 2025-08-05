using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using vFalcon.DataFeeds;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Services.Service
{
    public class VatsimDataService
    {
        private readonly DispatcherTimer refreshTimer = new();
        private readonly PilotService pilotService;
        private readonly Profile profile;
        private readonly Action invalidateCanvas;

        public VatsimDataService(PilotService pilotService, Profile profile, Action invalidateCanvas)
        {
            this.pilotService = pilotService;
            this.profile = profile;
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
            try
            {
                JObject? dataFeed = await VatsimDataFeed.AsyncGet();
                if (dataFeed == null) return;

                var transceiverFrequencies = await VatsimDataFeed.LoadTransceiverFrequenciesAsync();
                string? sectorFreq = await vNasDataFeed.GetArtccFrequencyAsync(profile.ArtccId, string.Empty);//FIXME

                await Task.Run(() =>
                {
                    pilotService.UpdateFromDataFeed(dataFeed, transceiverFrequencies, sectorFreq);
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
