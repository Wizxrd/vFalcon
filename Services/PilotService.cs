using Newtonsoft.Json.Linq;
using SkiaSharp;
using vFalcon.DataFeeds;
using vFalcon.Models;
using vFalcon.Utils;

namespace vFalcon.Services;

public class PilotService
{
    public Dictionary<string, Pilot> Pilots = new();
    private ScheduledFunction Scheduler { get; set; }
    public DisplayState DisplayState { get; set; }
    public bool ForceRefresh = false;

    private static float clickRadius = 10f;
    private bool stopped = false;
    private bool disposed = false;

    public PilotService()
    {
        Scheduler = new ScheduledFunction(Refresh, (int)Schedulers.RefreshRate.PilotService, true);
    }

    public async void Start()
    {
        JObject? dataFeed = await VatsimDataFeed.GetDataFeed();
        if (dataFeed == null) return;
        DateTime dataFeedUtc = (DateTime)dataFeed?["general"]?["update_timestamp"];
        DateTime nowUtc = DateTime.UtcNow;
        TimeSpan cycle = TimeSpan.FromSeconds(15);
        TimeSpan elapsed = nowUtc - dataFeedUtc;
        TimeSpan delay;
        if (elapsed < TimeSpan.Zero) delay = -elapsed;
        else
        {
            var remainder = TimeSpan.FromTicks(elapsed.Ticks % cycle.Ticks);
            delay = cycle - remainder;
            if (delay == cycle) delay = TimeSpan.Zero;
        }
        if (delay > TimeSpan.Zero)
        {
            Logger.Debug("PilotService.Start", $"Delayed: {delay.TotalSeconds.ToString("F3")}");
            Refresh();
            await Task.Delay(delay);
        }
        stopped = false;
        disposed = false;
        Scheduler.Start();
    }
    
    public void Stop()
    {
        stopped = true;
        Scheduler.Stop();
    }

    public void Dispose()
    {
        disposed = true;
        Scheduler.Dispose();
    }

    public Pilot? IsPilotClickedOn(SKPoint point)
    {
        if (Pilots.Count == 0) return null;
        foreach (Pilot pilot in Pilots.Values)
        {
            if (pilot == null) continue;
            SKPoint pilotScreenPos = ScreenMap.CoordinateToScreen(DisplayState.Width, DisplayState.Height, DisplayState.Scale, DisplayState.PanOffset, pilot.Latitude, pilot.Longitude);
            float dx = pilotScreenPos.X - point.X;
            float dy = pilotScreenPos.Y - point.Y;
            double distanceSquared = Math.Sqrt(dx * dx + dy * dy);

            if (distanceSquared <= clickRadius)
            {
                return pilot;
            }
        }
        return null;
    }

    public void ResetDatablocksToDefault()
    {
        foreach (Pilot pilot in Pilots.Values.ToList())
        {
            if (!pilot.ForcedDatablockType && pilot.DatablockType != App.Profile.GeneralSettings.DefaultDatablockType)
            {
                pilot.DatablockType = App.Profile.GeneralSettings.DefaultDatablockType;
            }
        }
    }

    public void IncreaseVelocityVector()
    {
        if (App.Profile.AppearanceSettings.VelocityVector == 0)
        {
            App.Profile.AppearanceSettings.VelocityVector = 1;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            return;
        }
        if (App.Profile.AppearanceSettings.VelocityVector < 8)
        {
            App.Profile.AppearanceSettings.VelocityVector *= 2;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public void DecreaseVelocityVector()
    {
        if (App.Profile.AppearanceSettings.VelocityVector > 0)
        {
            App.Profile.AppearanceSettings.VelocityVector /= 2;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public void CycleDatablockPosition(Pilot pilot)
    {
        switch (pilot.DatablockPosition)
        {
            case DatablockPosition.SouthWest:
                {
                    pilot.DatablockPosition = DatablockPosition.South;
                    break;
                }
            case DatablockPosition.South:
                {
                    pilot.DatablockPosition = DatablockPosition.SouthEast;
                    break;
                }
            case DatablockPosition.SouthEast:
                {
                    pilot.DatablockPosition = DatablockPosition.West;
                    break;
                }
            case DatablockPosition.West:
                {
                    pilot.DatablockPosition = DatablockPosition.Default;
                    break;
                }
            case DatablockPosition.Default:
                {
                    pilot.DatablockPosition = DatablockPosition.East;
                    break;
                }
            case DatablockPosition.East:
                {
                    pilot.DatablockPosition = DatablockPosition.NorthWest;
                    break;
                }
            case DatablockPosition.NorthWest:
                {
                    pilot.DatablockPosition = DatablockPosition.North;
                    break;
                }
            case DatablockPosition.North:
                {
                    pilot.DatablockPosition = DatablockPosition.NorthEast;
                    break;
                }
            case DatablockPosition.NorthEast:
                {
                    pilot.DatablockPosition = DatablockPosition.SouthWest;
                    break;
                }
        }
    }

    public void CycleLeaderLineLength(Pilot pilot)
    {
        switch (pilot.LeaderLineLength)
        {
            case 0:
                {
                    pilot.LeaderLineLength = 1;
                    break;
                }
            case 1:
                {
                    pilot.LeaderLineLength = 2;
                    break;
                }
            case 2:
                {
                    pilot.LeaderLineLength = 4;
                    break;
                }
            case 4:
                {
                    pilot.LeaderLineLength = 8;
                    break;
                }
            case 8:
                {
                    pilot.LeaderLineLength = 0;
                    break;
                }
        }
    }

    public void CycleDriSize(Pilot pilot)
    {
        switch (pilot.DriSize)
        {
            case 1:
                {
                    pilot.DriSize = 3;
                    break;
                }
            case 3:
                {
                    pilot.DriSize = 5;
                    break;
                }
            case 5:
                {
                    pilot.DriSize = 10;
                    break;
                }
            case 10:
                {
                    pilot.DriSize = 15;
                    break;
                }
            case 15:
                {
                    pilot.DriSize = 20;
                    break;
                }
            case 20:
                {
                    pilot.DriSize = 25;
                    break;
                }
            case 25:
                {
                    pilot.DriSize = 30;
                    break;
                }
            case 30:
                {
                    pilot.DriSize = 1;
                    break;
                }
        }
    }

    public async Task SarlaccPit(JArray pilots)
    {
        try
        {
            await Task.Run(() =>
            {
                var dataFeedCallsigns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (JObject obj in pilots)
                {
                    var callsign = (string?)obj["callsign"];
                    if (!string.IsNullOrEmpty(callsign))
                        dataFeedCallsigns.Add(callsign);
                }
                foreach (var pilot in Pilots.Values.ToList())
                {
                    if (!dataFeedCallsigns.Contains(pilot.Callsign))
                    {
                        Pilots.Remove(pilot.Callsign);
                    }
                }
            });
        }
        catch(Exception ex)
        {
            Logger.Error("PilotService.SarlaccPit", ex.ToString());
        }
    }

    public async Task Refresh()
    {
        try
        {
            if (stopped || disposed || App.MainWindowViewModel.IsPlayback || App.MainWindowViewModel == null) return;
            JObject? dataFeed = await VatsimDataFeed.GetDataFeed();
            if (dataFeed == null) return;
            if (ForceRefresh) ForceRefresh = false;
            Dictionary<string, string> transceiverFrequencies = await VatsimDataFeed.GetTransceiversAsync();
            JArray dataFeedPilots = (JArray)dataFeed["pilots"];
            foreach (JObject pilot in dataFeedPilots)
            {
                SKPoint pilotPosition = ScreenMap.CoordinateToScreen(DisplayState.Width, DisplayState.Height, DisplayState.Scale, DisplayState.PanOffset, (double)pilot["latitude"], (double)pilot["longitude"]);
                if (ScreenMap.PointInCoordinatePolygon(pilotPosition, App.MainWindowViewModel.SurveillanceAoi, DisplayState.Width, DisplayState.Height, DisplayState.Scale, DisplayState.PanOffset))
                {
                    string callsign = (string)pilot["callsign"];
                    string tunedFrequency = transceiverFrequencies.ContainsKey(callsign) ? transceiverFrequencies[callsign] : string.Empty;//transceiverFrequencies.TryGetValue(callsign, out var mappedFrequency) ? mappedFrequency : string.Empty;
                    JObject flightPlan = pilot["flight_plan"] as JObject;


                    bool isOnActivePositionFrequency = App.Profile.ActivePositions
                        .Values<string>()
                        .Any(f => f == tunedFrequency);

                    if (!Pilots.TryGetValue(callsign, out var existingPilot))
                    {
                        var acType = flightPlan?["aircraft_short"]?.Value<string>();
                        var newPilot = new Pilot
                        {
                            CID = (int)pilot["cid"],
                            Name = (string)pilot["name"],
                            Callsign = callsign,
                            Transponder = (string)pilot["transponder"],
                            LogOnTime = (DateTime)pilot["logon_time"],
                            Frequency = tunedFrequency,
                            CwtCode = Cwt.GetCodeFromType(acType),
                            FlightPlan = flightPlan,
                            DatablockType = App.Profile.GeneralSettings.DefaultDatablockType
                        };
                        Pilots[callsign] = newPilot;
                        existingPilot = newPilot;
                    }
                    existingPilot.Latitude = (double)pilot["latitude"];
                    existingPilot.Altitude = (int)pilot["altitude"];
                    existingPilot.GroundSpeed = (int)pilot["groundspeed"];
                    existingPilot.Heading = (int)pilot["heading"];
                    existingPilot.Longitude = (double)pilot["longitude"];
                    existingPilot.FlightPlan = flightPlan;
                    existingPilot.LastUpdated = (DateTime)pilot["last_updated"];
                    existingPilot.Frequency = tunedFrequency;
                    existingPilot.History.Add(new Coordinate { Lat = existingPilot.Latitude, Lon = existingPilot.Longitude });
                    if (isOnActivePositionFrequency) existingPilot.FullDatablockEnabled = true;
                    else if (existingPilot.FullDatablockEnabled) existingPilot.FullDatablockEnabled = false;
                    if (existingPilot.ForcedFullDatablock == true) existingPilot.FullDatablockEnabled = true;
                }
            }
            if (App.MainWindowViewModel.IsRecording)
            {
                App.MainWindowViewModel.RecordingService.UpdateAndSave(Pilots);
                App.MainWindowViewModel.UpdateRecordingDuration();
            }
            App.MainWindowViewModel.LastDataFeedUpdate = $"{DateTime.UtcNow:HH\\:mm\\:ss}z";
            await SarlaccPit(dataFeedPilots);
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
        catch(Exception ex)
        {
            Logger.Error("PilotService.Refresh", ex.ToString());
        }
    }
}
