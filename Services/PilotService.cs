using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Services
{
    public class PilotService
    {
        public Dictionary<string, Pilot> Pilots { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void UpdateFromDataFeed(JObject dataFeed, Dictionary<string, string> transceiverFrequencies, string? sectorFreq, ArtccBox? artccBox)
        {
            JArray? pilots = (JArray?)dataFeed["pilots"];
            if (pilots == null) return;

            foreach (var pilot in pilots)
            {
                double lat = (double)pilot["latitude"];
                double lon = (double)pilot["longitude"];

                if (artccBox == null || !artccBox.Contains(lat, lon))
                    continue;

                string callsign = (string)pilot["callsign"];
                bool fullDataBlock = transceiverFrequencies.TryGetValue(callsign, out var freq) && freq == sectorFreq;

                if (!Pilots.TryGetValue(callsign, out var p))
                {
                    p = new Pilot
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
                        LogOnTime = (DateTime)pilot["logon_time"]
                    };
                    Pilots[callsign] = p;
                }

                p.Latitude = lat;
                p.Longitude = lon;
                p.Altitude = (int)pilot["altitude"];
                p.GroundSpeed = (int)pilot["groundspeed"];
                p.Heading = (int)pilot["heading"];
                p.FlightPlan = pilot["flight_plan"] as JObject;
                p.LastUpdated = (DateTime)pilot["last_updated"];
                p.FullDataBlock = fullDataBlock;
            }

            // Remove stale
            var stale = Pilots
                .Where(kvp => (DateTime.UtcNow - kvp.Value.LastUpdated).TotalSeconds > 90)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var c in stale)
                Pilots.Remove(c);
        }
    }
}
