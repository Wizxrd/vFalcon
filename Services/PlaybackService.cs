using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Service;
using vFalcon.ViewModels;

namespace vFalcon.Services
{
    public class PlaybackService
    {
        private RadarViewModel? radarViewModel;
        private DispatcherTimer playbackTimer = new DispatcherTimer();
        private JObject replayJson = new JObject();
        private int tick;

        public void StartPlayback(RadarViewModel radarViewModel, string recordingPath)
        {
            tick = 0;
            this.radarViewModel = radarViewModel;
            replayJson = JObject.Parse(File.ReadAllText(recordingPath));
            playbackTimer.Interval = TimeSpan.FromSeconds(1);
            playbackTimer.Tick += PlaybackTimerTick;
            playbackTimer.Start();
        }

        public void StopPlayback()
        {
            playbackTimer.Tick -= PlaybackTimerTick;
            playbackTimer.Stop();
            radarViewModel = null;
            tick = 0;
        }

        private void PlaybackTimerTick(object? sender, EventArgs? e)
        {
            foreach (var kv in replayJson) // JObject: KeyValuePair<string, JToken>
            {
                string callsign = kv.Key;
                var data = kv.Value as JObject;
                if (data is null) continue;

                var history = data["History"] as JArray;
                if (history is null || tick < 0 || tick >= history.Count)
                {
                    if (radarViewModel.pilotService.Pilots.Remove(callsign))
                        Logger.Debug("Removing", callsign);
                    continue;
                }

                var coords = history[tick] as JArray;
                if (coords is null || coords.Count < 2) continue;

                double lat = coords[0].Value<double>();
                double lon = coords[1].Value<double>();

                if (!radarViewModel.pilotService.Pilots.TryGetValue(callsign, out var pilot))
                {
                    pilot = new Pilot
                    {
                        Callsign = callsign,
                        FlightPlan = data["FlightPlan"] as JObject ?? new JObject()
                    };
                    radarViewModel.pilotService.Pilots[callsign] = pilot;
                }

                pilot.Latitude = lat;
                pilot.Longitude = lon;

                var altArr = data["Altitude"] as JArray;
                var gsArr = data["GroundSpeed"] as JArray;
                var hdgArr = data["Heading"] as JArray;

                if (altArr is not null && tick < altArr.Count) pilot.Altitude = altArr[tick].Value<int>();
                if (gsArr is not null && tick < gsArr.Count) pilot.GroundSpeed = gsArr[tick].Value<int>();
                if (hdgArr is not null && tick < hdgArr.Count) pilot.Heading = hdgArr[tick].Value<int>();

                if (pilot.ForcedFullDataBlock) pilot.FullDataBlock = true;

                pilot.History ??= new List<(double, double)>();
                var last = pilot.History.Count > 0 ? pilot.History[^1] : (double.NaN, double.NaN);
                if (last.Item1 != lat || last.Item2 != lon)
                {
                    pilot.History.Add((lat, lon));
                    if (pilot.History.Count > 6) pilot.History.RemoveAt(0); // TODO: make 6 configurable
                }
            }

            tick++;
            radarViewModel?.Redraw();
        }

    }
}
