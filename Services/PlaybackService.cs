using Newtonsoft.Json.Linq;
using System.IO;
using vFalcon.Models;
using vFalcon.UI.ViewModels.Controls;
using vFalcon.Utils;
namespace vFalcon.Services;

public class PlaybackService
{
    public ScheduledFunction Scheduler { get; set; }
    public JObject ReplayJson = new JObject();
    public bool Paused = false;
    public int Tick;
    private int speedMultiplier = 1;
    private int tickStep = 1;
    private bool stopped = false;
    private bool disposed = false;

    public PlaybackService()
    {
        Scheduler = new(Refresh, 1, true);
    }

    public int GetTotalTickCount()
    {
        if (ReplayJson == null) return 0;
        return (int)ReplayJson["TickCount"];
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
        ReplayJson = JObject.Parse(File.ReadAllText(recordingPath));
        return ReplayJson;
    }

    public void Start()
    {
        Tick = 0;
        stopped = false;
        disposed = false;
        Scheduler.Start();
    }

    public void Dispose()
    {
        disposed = true;
        stopped = true;
        ReplayJson = new JObject();
        Scheduler.Dispose();
    }

    public void Stop()
    {
        stopped = true;
        ReplayJson = new JObject();
        Scheduler.Stop();
        Tick = 0;
    }

    public void Pause()
    {
        Paused = true;
    }

    public void Play()
    {
        Paused = false;
    }

    public async Task Refresh()
    {
        if (stopped || disposed) return;
        try
        {
            var total = ReplayJson["TickCount"]?.Value<int?>() ?? 0;
            if (Tick >= total) return;

            JObject pilotsObj = (JObject)ReplayJson["Pilots"];
            if (pilotsObj is null) return;

            foreach (var prop in pilotsObj)
            {
                string callsign = prop.Key;
                var data = prop.Value as JObject;
                if (data is null) continue;

                int startTick = data["StartTick"]?.Value<int?>() ?? 0;
                int frame = Tick - startTick;

                var history = data["History"] as JArray;
                if (history is null || frame < 0 || frame >= history.Count)
                {
                    App.MainWindowViewModel.PilotService.Pilots.Remove(callsign);
                    continue;
                }

                var coords = history[frame] as JArray;
                if (coords is null || coords.Count < 2) continue;

                double lat = (double)coords[0];
                double lon = (double)coords[1];
                if (!App.MainWindowViewModel.PilotService.Pilots.TryGetValue(callsign, out var pilot))
                {
                    pilot = new Pilot
                    {
                        Callsign = callsign,
                        FlightPlan = (data["FlightPlan"] as JObject) ?? new JObject()
                    };
                    App.MainWindowViewModel.PilotService.Pilots[callsign] = pilot;
                }

                var acType = pilot.FlightPlan?["aircraft_short"]?.Value<string>();
                pilot.CwtCode = Cwt.GetCodeFromType(acType);
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

                bool isOnActivePositionFrequency = App.Profile.PositionsSettings.ActivePositions
                    .Values<string>()
                    .Any(f => f == tunedFrequency);
                pilot.History.Add(new Coordinate { Lat = lat, Lon = lon });
            }

            if (!Paused)
            {
                int next = Tick + tickStep;
                var totalTick = ReplayJson["TickCount"]?.Value<int?>() ?? 0;
                Tick = next >= totalTick ? totalTick : next;
            }
            ReplayControlsViewModel rcvm = App.MainWindowView.ReplayControlsView.DataContext as ReplayControlsViewModel;
            rcvm.SliderValueTick = Tick;
            var ts = TimeSpan.FromSeconds(Tick * 15);
            rcvm.ElapsedTimeTick = $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
        catch (Exception ex)
        {
            Logger.Error("PlaybackTimerTick", ex.ToString());
        }
    }
}
