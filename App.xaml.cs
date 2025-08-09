using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string version = "0.0.8";
        private static Mutex mutex;
        const string appName = "vFalcon";
        bool createdNew;

        public App()
        {
            mutex = new Mutex(true, appName, out createdNew);
            Logger.DebugMode = true;
            Logger.LogLevelThreshold = LogLevel.Trace;
            Logger.Info("App", $"Launching vFalcon v{version}");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!createdNew)
            {
                Shutdown();
                return;
            }
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
