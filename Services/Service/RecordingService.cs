using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.Services.Service
{
    public class RecordingService
    {
        private static string recordingName = string.Empty;
        public Dictionary<string, Recording> recordingData = new Dictionary<string, Recording>();

        public void Start()
        {
            recordingName = UniqueHash.Generate();
            recordingData = new Dictionary<string, Recording>();
        }

        public void Stop()
        {
            recordingName = string.Empty;
        }

        public void Update(Dictionary<string, Pilot> pilots)
        {
            if (recordingName == string.Empty) recordingName = UniqueHash.Generate();
            foreach (var pilot in pilots.Values.ToList())
            {
                if (!recordingData.ContainsKey(pilot.Callsign))
                {
                    recordingData[pilot.Callsign] = new Recording
                    {
                        FlightPlan = pilot.FlightPlan,
                        Altitude = new JArray
                        {
                            {pilot.Altitude }
                        },
                        GroundSpeed = new JArray
                        {
                            {pilot.GroundSpeed }
                        },
                        Heading = new JArray
                        {
                            {pilot.Heading }
                        },
                        History = new JArray
                        {
                            new JArray(pilot.Latitude, pilot.Longitude)
                        }
                    };
                }
                else
                {
                    Recording existingRecording = recordingData[pilot.Callsign];
                    existingRecording.Altitude.Add(pilot.Altitude);
                    existingRecording.GroundSpeed.Add(pilot.GroundSpeed);
                    existingRecording.Heading.Add(pilot.Heading);
                    existingRecording.History.Add(new JArray(pilot.Latitude, pilot.Longitude));
                }
            }
            Save();
        }

        private void Save()
        {
            try
            {
                // Convert the dictionary to JSON
                string jsonData = JsonConvert.SerializeObject(recordingData, Formatting.Indented);
                string folder = Loader.LoadFolder("Recordings");
                File.WriteAllText(Loader.LoadFile(folder, $"{recordingName}.json"), jsonData);
                Logger.Info("Recording.Save", $"Saved recording data");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the file saving process
                Console.WriteLine($"Error saving pilot data: {ex.Message}");
            }
        }
    }
}
