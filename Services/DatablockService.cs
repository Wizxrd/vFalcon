using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Models;
using vFalcon.Utils;

namespace vFalcon.Services;

public class DatablockService
{
    private int cycleMax = 3;
    private int currentCycle = 0;
    private ScheduledFunction Scheduler;

    public DatablockService()
    {
        Scheduler = new ScheduledFunction(CycleDatablock, (int)Schedulers.RefreshRate.DatablockService);
    }

    public void Start() => Scheduler.Start();
    public void Stop() => Scheduler.Stop();
    public void Dispose() => Scheduler.Dispose();

    private async Task CycleDatablock()
    {
        if (App.MainWindowViewModel.PilotService == null) return;
        List<Pilot> pilots = App.MainWindowViewModel.PilotService.Pilots.Values.ToList();
        foreach (Pilot pilot in pilots)
        {
            if (pilot == null) continue;
            string arrival = pilot.FlightPlan?["arrival"]?.ToString();
            if (!string.IsNullOrEmpty(arrival) && arrival.Length > 1 && arrival.StartsWith("K"))
                arrival = arrival.Substring(1);
            string departure = pilot.FlightPlan?["departure"]?.ToString();
            if (!string.IsNullOrEmpty(departure) && departure.Length > 1 && departure.StartsWith("K"))
                departure = departure.Substring(1);
            int gsTens = pilot.GroundSpeed / 10;
            string groundSpeed = $"{gsTens:D2} {pilot.CwtCode}^";
            Coordinate pilotCoordinate = new Coordinate { Lat = pilot.Latitude, Lon = pilot.Longitude };
            Coordinate? arrivalCoordinate = App.MainWindowViewModel.RouteService.GetAirportCoords(arrival);
            Coordinate? departureCoordinate = App.MainWindowViewModel.RouteService.GetAirportCoords(departure);
            double distanceToArrival = double.MaxValue;
            double distanceFromDeparture = double.MaxValue;
            if (arrivalCoordinate != null) distanceToArrival = ScreenMap.DistanceInNM(pilotCoordinate, arrivalCoordinate);
            if (departureCoordinate != null) distanceFromDeparture = ScreenMap.DistanceInNM(pilotCoordinate, departureCoordinate);
            if (distanceToArrival <= 35)
            {
                if (pilot.DatablockType != DatablockType.Stars && !pilot.ForcedDatablockType && App.Profile.GeneralSettings.AutoDatablock) pilot.DatablockType = DatablockType.Stars;
                switch (currentCycle)
                {
                    case 0:
                        pilot.StarsDatablockRow2Col1 = arrival;
                        pilot.StarsDatablockRow2Col2 = pilot.FlightPlan?["aircraft_short"]?.ToString();
                        break;
                    case 1:
                        pilot.StarsDatablockRow2Col1 = (pilot.Altitude / 100).ToString("D3");
                        pilot.StarsDatablockRow2Col2 = groundSpeed;
                        break;
                    case 2:
                        pilot.StarsDatablockRow2Col1 = arrival;
                        pilot.StarsDatablockRow2Col2 = groundSpeed;
                        break;
                    case 3:
                        pilot.StarsDatablockRow2Col1 = (pilot.Altitude / 100).ToString("D3");
                        pilot.StarsDatablockRow2Col2 = groundSpeed;
                        break;
                }
            }
            else if (distanceFromDeparture <= 35)
            {
                if (pilot.DatablockType != DatablockType.Stars && !pilot.ForcedDatablockType && App.Profile.GeneralSettings.AutoDatablock) pilot.DatablockType = DatablockType.Stars;
                JArray childFacilities = (JArray)App.Artcc.facility["childFacilities"];
                JObject matchedChild = new JObject();
                foreach (JObject child in childFacilities)
                {
                    foreach (JObject child2 in child["childFacilities"])
                    {
                        if (departure == (string)child2["id"])
                        {
                            matchedChild = child;
                            break;
                        }
                    }
                }
                var match = matchedChild;
                var starsConfig = match?["starsConfiguration"] as JObject;
                var primScratch = starsConfig?["primaryScratchpadRules"] as JArray;

                string dep = (string)pilot.FlightPlan["departure"];
                string arr = (string)pilot.FlightPlan["arrival"];
                string route = (string)pilot.FlightPlan["route"];

                string template = Scratchpad.GetTemplate(primScratch, route, dep, arr);

                if (string.IsNullOrWhiteSpace(template))
                {
                    string candidate = !string.IsNullOrWhiteSpace(arr) ? arr
                                     : !string.IsNullOrWhiteSpace(dep) ? dep
                                     : string.Empty;

                    template = (candidate.Length == 4 && (candidate[0] == 'K' || candidate[0] == 'k'))
                        ? candidate.Substring(1)
                        : candidate;
                }
                switch (currentCycle)
                {
                    case 0:
                        pilot.StarsDatablockRow2Col1 = template;
                        pilot.StarsDatablockRow2Col2 = pilot.FlightPlan?["aircraft_short"]?.ToString();
                        break;
                    case 1:
                        pilot.StarsDatablockRow2Col1 = (pilot.Altitude / 100).ToString("D3");
                        pilot.StarsDatablockRow2Col2 = groundSpeed;
                        break;
                    case 2:
                        pilot.StarsDatablockRow2Col1 = template;
                        pilot.StarsDatablockRow2Col2 = groundSpeed;
                        break;
                    case 3:
                        pilot.StarsDatablockRow2Col1 = (pilot.Altitude / 100).ToString("D3");
                        pilot.StarsDatablockRow2Col2 = groundSpeed;
                        break;
                }
            }
            else
            {
                if (pilot.ForcedDatablockType == false && App.Profile.GeneralSettings.AutoDatablock) pilot.DatablockType = DatablockType.Eram;
                else if (pilot.DatablockType == DatablockType.Stars)
                {
                    string acShort = pilot.FlightPlan?["aircraft_short"]?.ToString();
                    acShort = string.IsNullOrWhiteSpace(acShort) ? groundSpeed : acShort;
                    switch (currentCycle)
                    {
                        case 0:
                            pilot.StarsDatablockRow2Col1 = (pilot.Altitude / 100).ToString("D3");
                            pilot.StarsDatablockRow2Col2 = acShort;
                            break;
                        case 1:
                            pilot.StarsDatablockRow2Col1 = (pilot.Altitude / 100).ToString("D3");
                            pilot.StarsDatablockRow2Col2 = $"{gsTens:D2}EF";
                            break;
                        case 2:
                            pilot.StarsDatablockRow2Col1 = (pilot.Altitude / 100).ToString("D3");
                            pilot.StarsDatablockRow2Col2 = $"{gsTens:D2}EF";
                            break;
                        case 3:
                            pilot.StarsDatablockRow2Col1 = (pilot.Altitude / 100).ToString("D3");
                            pilot.StarsDatablockRow2Col2 = $"{gsTens:D2}EF";
                            break;
                    }
                }
            }
        }
        currentCycle++;
        if (currentCycle > cycleMax) currentCycle = 0;
        App.MainWindowViewModel.GraphicsEngine.RequestRender();
    }
}
