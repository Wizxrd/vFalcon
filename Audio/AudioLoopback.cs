using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.Audio
{
    public class AudioLoopback : IDisposable
    {
        private WasapiLoopbackCapture capture;
        private WaveFileWriter writer;

        public void Start(string outputPath)
        {
            if (writer != null) return;

            capture = new WasapiLoopbackCapture(); // default output device
            writer = new WaveFileWriter(outputPath, capture.WaveFormat);

            capture.DataAvailable += (s, a) =>
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);
                writer.Flush();
            };

            capture.RecordingStopped += (s, a) =>
            {
                writer?.Dispose();
                writer = null;
                capture?.Dispose();
                capture = null;
            };

            capture.StartRecording();
        }

        public void Stop() => capture?.StopRecording();

        public void Dispose() => Stop();
    }
}
