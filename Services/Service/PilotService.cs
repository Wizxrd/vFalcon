using NAudio.Gui;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.ViewModels;
using vFalcon.Models;
using vFalcon.Helpers;
using System.Windows.Forms;

namespace vFalcon.Services.Service
{
    public class PilotService
    {
        private EramViewModel eramViewModel;
        private Artcc artcc;
        public RecordingService recordingService = new();
        public Dictionary<string, Pilot> Pilots { get; } = new(StringComparer.OrdinalIgnoreCase);

        public PilotService(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            artcc = eramViewModel.artcc; 
        }

        public void UpdateFromDataFeed(JObject dataFeed, Dictionary<string, string> transceiverFrequencies, string? sectorFreq)
        {
            JArray? pilots = (JArray?)dataFeed["pilots"];
            if (pilots == null) return;

            foreach (var pilot in pilots)
            {
                if ((int)pilot["groundspeed"] < 30) continue;
                double lat = (double)pilot["latitude"];
                double lon = (double)pilot["longitude"];

                bool withinAsrRange = false;
                bool withinSurveillanceRange = false;
                foreach (JObject asr in (JArray)artcc.facility["eramConfiguration"]["asrSites"])
                {
                    JObject location = (JObject)asr["location"];
                    int range = (int)asr["range"];
                    if (ScreenMap.DistanceInNM(lat, lon, (double)location["lat"], (double)location["lon"]) <= range)
                    {
                        withinAsrRange = true;
                        break;
                    }
                }
                foreach (JObject facility in (JArray)artcc.facility["childFacilities"])
                {
                    var starsConfig = facility["starsConfiguration"];
                    if (starsConfig?["areas"] is JArray areas)
                    {
                        foreach (JObject area in areas)
                        {
                            JObject visibilityCenter = (JObject)area["visibilityCenter"];
                            int surveillanceRange = (int)area["surveillanceRange"];
                            if (visibilityCenter == null) continue;
                            if (ScreenMap.DistanceInNM(lat, lon, (double)visibilityCenter["lat"], (double)visibilityCenter["lon"]) <= surveillanceRange)
                            {
                                withinSurveillanceRange = true;
                            }
                        }
                    }
                }
                

                if (!withinAsrRange && !withinSurveillanceRange) continue;

                string callsign = (string)pilot["callsign"];
                bool fullDataBlock = transceiverFrequencies.TryGetValue(callsign, out var freq) && freq == sectorFreq;
                if (!Pilots.ContainsKey(callsign))
                {
                    Pilot newPilot = new Pilot
                    {
                        CID = (int)pilot["cid"],
                        Name = (string)pilot["name"],
                        Callsign = callsign,
                        Server = (string)pilot["server"],
                        PilotRating = (int)pilot["pilot_rating"],
                        MilitaryRating = (int)pilot["military_rating"],
                        Transponder = (string)pilot["transponder"],
                        QNHinHG = (double)pilot["qnh_i_hg"],
                        QNHmb = (int)pilot["qnh_mb"],
                        LogOnTime = (DateTime)pilot["logon_time"],
                    };
                    Pilots[callsign] = newPilot;
                }

                Pilot existingPilot = Pilots[callsign];
                existingPilot.Latitude = lat;
                existingPilot.Longitude = lon;
                existingPilot.Altitude = (int)pilot["altitude"];
                existingPilot.GroundSpeed = (int)pilot["groundspeed"];
                existingPilot.Heading = (int)pilot["heading"];
                existingPilot.FlightPlan = pilot["flight_plan"] as JObject;
                existingPilot.LastUpdated = (DateTime)pilot["last_updated"];
                existingPilot.FullDataBlock = fullDataBlock;

                if (existingPilot.ForcedFullDataBlock == true)
                {
                    existingPilot.FullDataBlock = true;
                }

                if (existingPilot.History == null)
                    existingPilot.History = new List<(double, double)>();

                var lastPoint = existingPilot.History.Count > 0 ? existingPilot.History[^1] : (double.NaN, double.NaN);
                if (lastPoint.Item1 != lat || lastPoint.Item2 != lon)
                {
                    if (existingPilot.History.Count <= 6) // FIXME history count in MasterToolbar!
                    {
                        existingPilot.History.Add((lat, lon));
                    }
                    else
                    {
                        existingPilot.History.Add((lat, lon));
                        existingPilot.History.RemoveAt(0);
                    }
                }
            }

            var stale = Pilots
                .Where(kvp => (DateTime.UtcNow - kvp.Value.LastUpdated).TotalSeconds > 90)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var c in stale)
            {
                Pilots.Remove(c);
                if (recordingService.recordingData.ContainsKey(c))
                {
                    Logger.Trace("Removing", c.ToString());
                    recordingService.recordingData.Remove(c);
                }
            }

            if (eramViewModel.IsRecording)
            {
                recordingService.Update(Pilots);
            }
        }
    }
}
