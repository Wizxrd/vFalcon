using NAudio.Wave;
using System;
using System.Threading;

namespace vFalcon.Audio
{
    public sealed class MicRecorder : IDisposable
    {
        private WaveInEvent waveIn;
        private WaveFileWriter writer;
        private readonly int sampleRate, channels;
        private readonly object _writeLock = new();
        private volatile int _pttDown;
        private readonly ManualResetEventSlim _stopped = new(false);

        public MicRecorder(int sampleRate = 44100, int channels = 1)
        {
            this.sampleRate = sampleRate;
            this.channels = channels;
        }

        public void SetPtt(bool isDown) =>
            Interlocked.Exchange(ref _pttDown, isDown ? 1 : 0);

        public void Start(string outputPath)
        {
            if (writer != null) return;

            _stopped.Reset();

            waveIn = new WaveInEvent
            {
                DeviceNumber = 0,
                WaveFormat = new WaveFormat(sampleRate, 16, channels),
                BufferMilliseconds = 50
            };

            writer = new WaveFileWriter(outputPath, waveIn.WaveFormat);

            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;
            waveIn.StartRecording();
        }

        private void OnDataAvailable(object sender, WaveInEventArgs a)
        {
            bool gateOpen = System.Threading.Volatile.Read(ref _pttDown) == 1;

            lock (_writeLock)
            {
                if (writer == null) return;

                if (!gateOpen)
                    Array.Clear(a.Buffer, 0, a.BytesRecorded); // silence

                writer.Write(a.Buffer, 0, a.BytesRecorded);
            }
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            waveIn.DataAvailable -= OnDataAvailable;
            waveIn.RecordingStopped -= OnRecordingStopped;

            lock (_writeLock)
            {
                writer?.Dispose();
                writer = null;
            }

            waveIn?.Dispose();
            waveIn = null;

            _stopped.Set();
        }

        public void Stop()
        {
            var wi = waveIn;
            if (wi == null) return;
            wi.StopRecording();
            _stopped.Wait(TimeSpan.FromSeconds(2));
        }

        public void Dispose()
        {
            Stop();
            _stopped.Dispose();
        }
    }
}
