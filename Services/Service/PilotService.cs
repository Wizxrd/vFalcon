using NAudio.Gui;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;

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

        private bool showAll = false;
        static long NormalizeHz(long value)
        {
            // If your data is already Hz (e.g., 126225000), this just returns it.
            // If you ever get kHz (e.g., 126225), this promotes to Hz.
            return value < 1_000_000 && value >= 1_000 ? value * 1_000 : value;
        }

        static string ToMhzString(long hz)
        {
            double mhz = NormalizeHz(hz) / 1_000_000d;
            return mhz.ToString("0.000", CultureInfo.InvariantCulture);
        }
        public void UpdateFromDataFeed(JObject dataFeed, Dictionary<string, string> transceiverFrequencies)
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
                if (eramViewModel.profile.DisplayType == "ERAM")
                {
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
                }
                foreach (JObject facility in (JArray)artcc.facility["childFacilities"])
                {
                    if (eramViewModel.profile.DisplayType == "STARS" && eramViewModel.profile.FacilityId != (string)facility["id"]) continue;
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
                

                if (!withinAsrRange && !withinSurveillanceRange && !showAll) continue;

                string callsign = (string)pilot["callsign"];

                string tunedFrequency = transceiverFrequencies.TryGetValue(callsign, out var mappedFrequency)
                    ? mappedFrequency
                    : string.Empty;

                bool isOnActiveSectorFrequency = eramViewModel.ActivatedSectors
                    .Select(kv => kv.Value)
                    .Any(sectorFrequency => string.Equals(sectorFrequency, tunedFrequency, StringComparison.OrdinalIgnoreCase));

                bool fullDataBlock = isOnActiveSectorFrequency;
                JObject flightPlan = pilot["flight_plan"] as JObject;
                if (!Pilots.TryGetValue(callsign, out var existingPilot))
                {
                    var acType = flightPlan?["aircraft_short"]?.Value<string>();
                    var newPilot = new Pilot
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
                        Frequency = tunedFrequency,
                        CwtCode = Cwt.GetCwtCodeFromType(acType),
                        FlightPlan = flightPlan,
                        FullDataBlock = fullDataBlock,
                        StarsSectorId = string.Empty,
                        DatablockType = eramViewModel.profile.DisplayType
                    };
                    if (isOnActiveSectorFrequency)
                    {
                        var childFacilities = (JArray)eramViewModel.artcc.facility["childFacilities"];
                        var match = childFacilities?.FirstOrDefault(cf => (string)cf["id"] == eramViewModel.profile.FacilityId) as JObject;
                        var positions = match?["positions"] as JArray;
                        var pos = positions?
                            .OfType<JObject>()
                            .FirstOrDefault(p =>
                            {
                                var hz = p.Value<long?>("frequency");
                                if (!hz.HasValue) return false;
                                return string.Equals(ToMhzString(hz.Value), newPilot.Frequency, StringComparison.Ordinal);
                            });

                        var starsConfiguration = pos?["starsConfiguration"] as JObject;
                        string sectorId = (string)starsConfiguration?["sectorId"];
                        newPilot.StarsSectorId = sectorId;
                    }
                    Pilots[callsign] = newPilot;
                    existingPilot = newPilot;
                }

                existingPilot.Latitude = (double)pilot["latitude"];
                existingPilot.Longitude = (double)pilot["longitude"];
                existingPilot.Altitude = (int)pilot["altitude"];
                existingPilot.GroundSpeed = (int)pilot["groundspeed"];
                existingPilot.Heading = (int)pilot["heading"];
                existingPilot.FlightPlan = flightPlan;
                existingPilot.LastUpdated = (DateTime)pilot["last_updated"];
                existingPilot.FullDataBlock = fullDataBlock;
                existingPilot.Frequency = tunedFrequency;
                existingPilot.StarsSectorId = string.Empty;

                if (isOnActiveSectorFrequency)
                {
                    var childFacilities = (JArray)eramViewModel.artcc.facility["childFacilities"];
                    var match = childFacilities?.FirstOrDefault(cf => (string)cf["id"] == eramViewModel.profile.FacilityId) as JObject;
                    var positions = match?["positions"] as JArray;
                    var pos = positions?
                        .OfType<JObject>()
                        .FirstOrDefault(p =>
                        {
                            var hz = p.Value<long?>("frequency");
                            if (!hz.HasValue) return false;
                            return string.Equals(ToMhzString(hz.Value), existingPilot.Frequency, StringComparison.Ordinal);
                        });

                    var starsConfiguration = pos?["starsConfiguration"] as JObject;
                    string sectorId = (string)starsConfiguration?["sectorId"];
                    existingPilot.StarsSectorId = sectorId;
                }

                if (existingPilot.ForcedFullDataBlock == true)
                {
                    existingPilot.FullDataBlock = true;
                }

                if (existingPilot.History == null)
                    existingPilot.History = new List<(double, double)>();

                var lastPoint = existingPilot.History.Count > 0 ? existingPilot.History[^1] : (double.NaN, double.NaN);
                if (lastPoint.Item1 != lat || lastPoint.Item2 != lon)
                {
                    if (existingPilot.History.Count <= eramViewModel.HistoryCount) // FIXME history count in MasterToolbar!
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
                    //recordingService.recordingData.Remove(c);
                }
            }

            if (eramViewModel.IsRecording)
            {
                recordingService.Update(Pilots);
            }
        }
    }
}
