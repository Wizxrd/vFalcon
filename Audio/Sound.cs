using NAudio.Wave;
using vFalcon.Helpers;

namespace vFalcon.Audio
{
    public class Sound
    {
        public static async Task Play(string fileName)
        {
            try
            {
                string filePath = Loader.LoadFile("Sounds", fileName);
                await Task.Run(() =>
                {
                    using (var audioFile = new AudioFileReader(filePath))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        audioFile.Volume = Math.Clamp(1.0f, 0.0f, 0.25f);
                        outputDevice.Init(audioFile);
                        outputDevice.Play();

                        Task.Run(async () =>
                        {
                            while (outputDevice.PlaybackState == PlaybackState.Playing)
                            {
                                await Task.Delay(10);
                            }
                        }).Wait();
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error("Sound.Play", ex.ToString());
            }
        }
    }
}
