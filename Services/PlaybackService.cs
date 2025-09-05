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
        private JObject replayJson = new JObject();
        private bool paused = false;
        public int Tick;
        public int speed = 1;
        private int baseMs = 1000;

        public int GetTotalTickCount()
        {
            if (replayJson == null) return 0;
            return (int)replayJson["TickCount"];
        }

        public void SetPlaybackSpeed(int speed)
        {
            if (speed < 1) speed = 1;
            this.speed = speed;
            playbackTimer.Interval = TimeSpan.FromMilliseconds(baseMs / speed);
        }

        public void StartPlayback(EramViewModel eramViewModel, Profile profile, string recordingPath)
        {
            Tick = 0;
            this.eramViewModel = eramViewModel;
            radarViewModel = eramViewModel.RadarViewModel;
            this.profile = profile;
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

        public async void PlaybackTimerTick(object? sender, EventArgs? e)
        {
            try
            {
                eramViewModel.UpdateReplayControls(Tick);
                var tickCountToken = replayJson["TickCount"];
                if (tickCountToken != null && Tick >= tickCountToken.Value<int>()) return;

                var pilotsObj = replayJson["Pilots"] as JObject;
                if (pilotsObj is null) return;

                string? sectorFreq = profile.ActivatedSectorFreq;

                foreach (var prop in pilotsObj.Properties())
                {
                    string callsign = prop.Name;
                    var data = prop.Value as JObject;
                    if (data is null) continue;

                    var history = data["History"] as JArray;
                    if (history is null || Tick < 0 || Tick >= history.Count)
                    {
                        if (radarViewModel.pilotService.Pilots.Remove(callsign))
                            Logger.Debug("Removing", callsign);
                        continue;
                    }

                    var coords = history[Tick] as JArray;
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

                    pilot.Latitude = lat;
                    pilot.Longitude = lon;

                    var altArr = data["Altitude"] as JArray;
                    var gsArr = data["GroundSpeed"] as JArray;
                    var hdgArr = data["Heading"] as JArray;
                    var freqArr = data["Frequency"] as JArray;

                    if (altArr is not null && Tick < altArr.Count && altArr[Tick]?.Type != JTokenType.Null)
                        pilot.Altitude = altArr[Tick]!.Value<int>();

                    if (gsArr is not null && Tick < gsArr.Count && gsArr[Tick]?.Type != JTokenType.Null)
                        pilot.GroundSpeed = gsArr[Tick]!.Value<int>();

                    if (hdgArr is not null && Tick < hdgArr.Count && hdgArr[Tick]?.Type != JTokenType.Null)
                        pilot.Heading = hdgArr[Tick]!.Value<int>();

                    if (!string.IsNullOrEmpty(sectorFreq) &&
                        freqArr is not null &&
                        Tick < freqArr.Count &&
                        freqArr[Tick]?.Type != JTokenType.Null &&
                        string.Equals(freqArr[Tick]!.Value<string>(), sectorFreq, StringComparison.OrdinalIgnoreCase))
                    {
                        pilot.FullDataBlock = true;
                    }
                    if (pilot.ForcedFullDataBlock) pilot.FullDataBlock = true;

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
                    Tick++;
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
