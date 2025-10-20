using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Service;

namespace vFalcon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string version = "1.0.0";
        private static Mutex mutex;
        const string appName = "vFalcon";
        bool createdNew;

        public App()
        {
            mutex = new Mutex(true, appName, out createdNew);
            Logger.DebugMode = false;
            Logger.LogLevelThreshold = LogLevel.Info;
            Logger.Info("App", $"Launching vFalcon v{version}");;
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
            for (int i = Application.Current.Windows.Count - 1; i >= 0; i--)
            {
                var w = Application.Current.Windows[i];
                if (w == null) continue;
                if (!w.Dispatcher.CheckAccess())
                    w.Dispatcher.Invoke(() => { if (w.IsLoaded) w.Close(); });
                else
                    if (w.IsLoaded) w.Close();
            }

            mutex?.ReleaseMutex();
            base.OnExit(e);
        }
    }
}
