using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Services.Service
{
    public class RecordingService
    {
        private static string recordingName = string.Empty;
        public Dictionary<string, Recording> recordingData = new Dictionary<string, Recording>();

        private DateTime startedUtc = DateTime.MinValue;
        private int tickCount = 0;
        private DateTime? lastUpdatedUtc;

        public void Start()
        {
            recordingName = UniqueHash.Generate();
            recordingData = new Dictionary<string, Recording>();
            startedUtc = DateTime.UtcNow;
            tickCount = 0;
        }

        public void Stop()
        {
            recordingName = string.Empty;
            startedUtc = DateTime.MinValue;
            tickCount = 0;
            lastUpdatedUtc = null;
        }

        public void Update(Dictionary<string, Pilot> pilots)
        {
            if (recordingName == string.Empty)
            {
                recordingName = UniqueHash.Generate();
                if (startedUtc == DateTime.MinValue) startedUtc = DateTime.UtcNow;
            }
            tickCount++;
            lastUpdatedUtc = DateTime.UtcNow;
            foreach (var pilot in pilots.Values.ToList())
            {
                if (!recordingData.ContainsKey(pilot.Callsign))
                {
                    recordingData[pilot.Callsign] = new Recording
                    {
                        FlightPlan = pilot.FlightPlan,
                        Altitude = new JArray { pilot.Altitude },
                        GroundSpeed = new JArray { pilot.GroundSpeed },
                        Heading = new JArray { pilot.Heading },
                        History = new JArray { new JArray(pilot.Latitude, pilot.Longitude) },
                        Frequency = new JArray { pilot.Frequency }
                    };
                }
                else
                {
                    var existing = recordingData[pilot.Callsign];
                    existing.Altitude.Add(pilot.Altitude);
                    existing.GroundSpeed.Add(pilot.GroundSpeed);
                    existing.Heading.Add(pilot.Heading);
                    existing.History.Add(new JArray(pilot.Latitude, pilot.Longitude));
                    existing.Frequency.Add(pilot.Frequency);
                }
            }

            Save();
        }

        private void Save()
        {
            try
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include
                };

                var pilotsObj = JObject.FromObject(recordingData, serializer);

                foreach (var prop in pilotsObj.Properties())
                {
                    if (prop.Value is JObject rec && rec["FullDataBlock"] == null)
                        rec["FullDataBlock"] = JValue.CreateNull();
                }

                var root = new JObject
                {
                    ["TickCount"] = tickCount,
                    ["LastUpdateTimeStamp"] = lastUpdatedUtc,
                    ["Pilots"] = pilotsObj
                };

                var folder = Loader.LoadFolder("Recordings");
                File.WriteAllText(Loader.LoadFile(folder, $"{recordingName}.json"), root.ToString(Formatting.Indented));
                Logger.Info("Recording.Save", "Saved recording data");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving pilot data: {ex.Message}");
            }
        }
    }
}
