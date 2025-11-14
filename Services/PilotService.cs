using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Net.NetworkInformation;
using System.Windows.Forms;
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
    private DateTime? lastUpdateTimestamp;

    public PilotService()
    {
        Scheduler = new ScheduledFunction(Refresh, (int)Schedulers.RefreshRate.PilotService, true);
    }

    public void Start() => Scheduler.Start();

    public void Stop() => Scheduler.Stop();

    public void Dispose() => Scheduler.Dispose();

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
            if (!pilot.ForcedDatablockType && pilot.DatablockType != App.Profile.DefaultDatablockType)
            {
                pilot.DatablockType = App.Profile.DefaultDatablockType;
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

    public async Task Refresh()
    {
        JObject? dataFeed = await VatsimDataFeed.GetDataFeed();
        if (dataFeed == null) return;
        string update_timestamp = dataFeed?["general"]?["update_timestamp"]?.ToString() ?? string.Empty;
        DateTime lastUpdateUtc;

        if (DateTime.TryParse(update_timestamp, out lastUpdateUtc))
        {
            lastUpdateUtc = lastUpdateUtc.AddMilliseconds(-lastUpdateUtc.Millisecond);
            if (lastUpdateTimestamp.HasValue && lastUpdateTimestamp.Value == lastUpdateUtc && !ForceRefresh)
            {
                Logger.Debug("PilotService.Refresh", "Skipping");
                return;
            }
            lastUpdateTimestamp = lastUpdateUtc;
        }
        if (ForceRefresh) ForceRefresh = false;
        Dictionary<string, string> transceiverFrequencies = await VatsimDataFeed.GetTransceiversAsync();
        JArray pilots = (JArray)dataFeed["pilots"];
        foreach (JObject pilot in pilots)
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
                        DatablockType = App.Profile.DefaultDatablockType
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
        App.MainWindowViewModel.GraphicsEngine.RequestRender();
    }
}
