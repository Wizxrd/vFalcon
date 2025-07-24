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
        string version = "0.0.1";

        public App()
        {
            Logger.DebugMode = true;
            Logger.LogLevelThreshold = LogLevel.Trace;
            Logger.Info("App", $"Launching vFalcon v{version}");
        }
    }
}
