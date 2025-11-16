using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using vFalcon.DataFeeds;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Service;
using vFalcon.ViewModels;

namespace vFalcon.Services
{
    public class PlaybackService
    {
        private RadarViewModel? radarViewModel;
        private EramViewModel eramViewModel;
        private Profile profile;
        private DispatcherTimer playbackTimer = new DispatcherTimer();
        public JObject replayJson = new JObject();
        private bool paused = false;
        public int Tick;
        public int speed = 15;
        private int baseMs = 1000;

        public int speedMultiplier = 1;
        private const int DefaultTicksPerSecond = 15;
        private const int MinIntervalMs = 15; // floor so the UI thread isn't hammered

        private const double BaseSecondsPerTick = 15.0; // 1× = 15 seconds per tick
        private int tickStep = 1;
        private void ApplyInterval()
        {
            int m = Math.Max(1, speedMultiplier);
            double secondsPerTick = BaseSecondsPerTick / m;
            playbackTimer.Stop();
            playbackTimer.Interval = TimeSpan.FromSeconds(secondsPerTick);
            playbackTimer.Start();
        }

        public int GetTotalTickCount()
        {
            if (replayJson == null) return 0;
            return (int)replayJson["TickCount"];
        }

        public void SetPlaybackSpeed(int multiplier)
        {
            if (multiplier < 1) multiplier = 1;
            if ((multiplier & (multiplier - 1)) != 0)
            {
                int p = 1;
                while (p < multiplier && p < 128) p <<= 1;
                int lower = p >> 1;
                multiplier = (p - multiplier) < (multiplier - Math.Max(1, lower)) ? p : Math.Max(1, lower);
                if (multiplier > 128) multiplier = 128;
            }
            if (multiplier > 128) multiplier = 128;

            speedMultiplier = multiplier;
            tickStep = speedMultiplier;
        }

        public JObject SetRecordingPath(string recordingPath)
        {
            replayJson = JObject.Parse(File.ReadAllText(recordingPath));
            return replayJson;
        }

        public void StartPlayback(EramViewModel eramViewModel, Profile profile)
        {
            Tick = 0;
            this.eramViewModel = eramViewModel;
            radarViewModel = eramViewModel.RadarViewModel;
            this.profile = profile;

            playbackTimer.Tick -= PlaybackTimerTick;
            playbackTimer.Tick += PlaybackTimerTick;

            playbackTimer.Interval = TimeSpan.FromSeconds(1);

            playbackTimer.Start();
            PlaybackTimerTick(null, null);
        }

        public void StopPlayback()
        {
            playbackTimer.Tick -= PlaybackTimerTick;
            playbackTimer.Stop();
            radarViewModel = null;
            replayJson = new JObject();
            Tick = 0;
        }

        public void Pause()
        {
            paused = true;
            playbackTimer.Stop();
        }

        public void Play()
        {
            paused = false;
            playbackTimer.Start();
        }
        public void PlaybackTimerTick(object? sender, EventArgs? e)
        {
            try
            {
                if (eramViewModel.RadarViewModel.pilotService == null) return;
                eramViewModel.UpdateReplayControls(Tick);

                var total = replayJson["TickCount"]?.Value<int?>() ?? 0;
                if (Tick >= total) return;

                var pilotsObj = replayJson["Pilots"] as JObject;
                if (pilotsObj is null) return;

                foreach (var prop in pilotsObj.Properties())
                {
                    string callsign = prop.Name;
                    var data = prop.Value as JObject;
                    if (data is null) continue;

                    int startTick = data["StartTick"]?.Value<int?>() ?? 0;
                    int frame = Tick - startTick;

                    var history = data["History"] as JArray;
                    if (history is null || frame < 0 || frame >= history.Count)
                    {
                        radarViewModel.pilotService.Pilots.Remove(callsign);
                        continue;
                    }

                    var coords = history[frame] as JArray;
                    if (coords is null || coords.Count < 2) continue;

                    double lat = coords[0].Value<double>();
                    double lon = coords[1].Value<double>();

                    if (!radarViewModel.pilotService.Pilots.TryGetValue(callsign, out var pilot))
                    {
                        pilot = new Pilot
                        {
                            Callsign = callsign,
                            FlightPlan = (data["FlightPlan"] as JObject) ?? new JObject()
                        };
                        radarViewModel.pilotService.Pilots[callsign] = pilot;
                    }

                    var acType = pilot.FlightPlan?["aircraft_short"]?.Value<string>();
                    if (string.IsNullOrEmpty(pilot.DatablockType)) pilot.DatablockType = eramViewModel.profile.DisplayType;
                    pilot.CwtCode = Cwt.GetCwtCodeFromType(acType);
                    pilot.Latitude = lat;
                    pilot.Longitude = lon;

                    var altArr = data["Altitude"] as JArray;
                    var gsArr = data["GroundSpeed"] as JArray;
                    var hdgArr = data["Heading"] as JArray;
                    var freqArr = data["Frequency"] as JArray;

                    if (altArr is not null && frame < altArr.Count && altArr[frame]?.Type != JTokenType.Null)
                        pilot.Altitude = altArr[frame]!.Value<int>();

                    if (gsArr is not null && frame < gsArr.Count && gsArr[frame]?.Type != JTokenType.Null)
                        pilot.GroundSpeed = gsArr[frame]!.Value<int>();

                    if (hdgArr is not null && frame < hdgArr.Count && hdgArr[frame]?.Type != JTokenType.Null)
                        pilot.Heading = hdgArr[frame]!.Value<int>();

                    string tunedFrequency =
                        (freqArr is not null && frame < freqArr.Count && freqArr[frame]?.Type == JTokenType.String)
                        ? freqArr[frame]!.Value<string>() ?? string.Empty
                        : string.Empty;

                    bool isOnActiveSectorFrequency = (eramViewModel.ActivatedSectors?.Values ?? Enumerable.Empty<string>())
                        .Any(sectorFrequency => string.Equals(sectorFrequency, tunedFrequency, StringComparison.OrdinalIgnoreCase));

                    pilot.FullDataBlock = isOnActiveSectorFrequency || pilot.ForcedFullDataBlock;

                    pilot.History ??= new List<(double, double)>();
                    var last = pilot.History.Count > 0 ? pilot.History[^1] : (double.NaN, double.NaN);
                    if (last.Item1 != lat || last.Item2 != lon)
                    {
                        pilot.History.Add((lat, lon));
                        if (pilot.History.Count > 6) pilot.History.RemoveAt(0);
                    }
                }

                if (!paused)
                {
                    int next = Tick + tickStep;
                    var totalTick = replayJson["TickCount"]?.Value<int?>() ?? 0;
                    Tick = next >= totalTick ? totalTick : next;
                }

                radarViewModel?.Redraw();
            }
            catch (Exception ex)
            {
                Logger.Error("PlaybackTimerTick", ex.Message);
            }
        }

    }
}
