using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using vFalcon.Models;
using vFalcon.Utils;

namespace vFalcon.Services;

public class RecordingService
{
    private string recordingGuid = string.Empty;
    private int tickCount = 0;
    private DateTime? lastUpdatedUtc;
    public JObject navData = new();
    private readonly SemaphoreSlim saveGate = new(1, 1);
    public Dictionary<string, PilotRecording> recordingData = new();
    public void Start()
    {
        string json = File.ReadAllText(PathFinder.GetFilePath("", "NavDataSerial.json"));
        navData = JObject.Parse(json);
        recordingGuid = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        recordingData = new();
        tickCount = 0;
    }

    public void Stop()
    {
        recordingGuid = string.Empty;
        tickCount = 0;
        lastUpdatedUtc = null;
    }

    public void UpdateAndSave(Dictionary<string, Pilot> pilots)
    {
        if (recordingGuid == string.Empty) return;
        tickCount++;
        lastUpdatedUtc = DateTime.UtcNow;
        foreach (Pilot pilot in pilots.Values)
        {
            if (!recordingData.ContainsKey(pilot.Callsign))
            {
                recordingData[pilot.Callsign] = new PilotRecording
                {
                    StartTick = Math.Max(0, tickCount - 1),
                    FlightPlan = pilot.FlightPlan,
                    Altitude = new List<int>{ pilot.Altitude },
                    GroundSpeed = new List<int>{ pilot.GroundSpeed },
                    Heading = new List<int>{ pilot.Heading },
                    History = new List<List<double>>{ new List<double>{ pilot.Latitude, pilot.Longitude }},
                    Frequency = new List<string> { pilot.Frequency }
                };
            }
            else
            {
                PilotRecording existing = recordingData[pilot.Callsign];
                existing.Altitude.Add(pilot.Altitude);
                existing.GroundSpeed.Add(pilot.GroundSpeed);
                existing.Heading.Add(pilot.Heading);
                existing.History.Add(new List<double> { pilot.Latitude, pilot.Longitude });
                existing.Frequency.Add(pilot.Frequency);
            }
        }
        Save();
    }
    private void Save()
    {
        saveGate.Wait();
        try
        {
            JsonSerializer serializer = new JsonSerializer
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include
            };

            JObject pilotsObj = JObject.FromObject(recordingData, serializer);
            JObject root = new JObject
            {
                ["NavData"] = navData,
                ["TickCount"] = tickCount,
                ["LastUpdateTimeStamp"] = lastUpdatedUtc,
                ["Pilots"] = pilotsObj
            };

            string folder = PathFinder.GetFolderPath("Recordings");
            string finalPath = PathFinder.GetFilePath(folder, $"{recordingGuid}.json");
            string tmpPath = finalPath + ".tmp";

            using (FileStream fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, FileOptions.WriteThrough))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonTextWriter jw = new JsonTextWriter(sw) { Formatting = Formatting.None })
            {
                root.WriteTo(jw);
                jw.Flush();
                sw.Flush();
                fs.Flush(true);
            }

            if (File.Exists(finalPath))
                File.Replace(tmpPath, finalPath, null);
            else
                File.Move(tmpPath, finalPath);

            Logger.Info("RecordingService.Save", "Saved recording data");
        }
        catch (Exception ex)
        {
            Logger.Error("RecordingService.Save", $"Error saving pilot data: {ex.Message}");

        }
        finally
        {
            saveGate.Release();
        }
    }
}
