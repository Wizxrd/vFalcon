using System;
using System.Configuration;
using System.Data;
using System.Windows;
using vFalcon.Models;
using vFalcon.UI.ViewModels;
using vFalcon.UI.Views;
using vFalcon.UI.Views.Manager;
using vFalcon.Utils;
namespace vFalcon;
public partial class App : Application
{
    public static string Version { get; } = "1.0.1";
    public static Profile Profile { get; set; }
    public static Artcc Artcc { get; set; }
    public static ViewManager ViewManager { get; set; } = new();
    public static MainWindowView MainWindowView { get; set; }
    public static MainWindowViewModel MainWindowViewModel { get; set; }

    private static Mutex Mutex = new();
    private const string appName = "vFalcon";
    private bool createdNew;

    public App()
    {
        Logger.DebugMode = true;
        Logger.LogLevelThreshold = LogLevel.Trace;
        Logger.Info("App", $"Launching vFalcon v{Version}");
        Mutex = new Mutex(true, appName, out createdNew);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        if (!createdNew)
        {
            Message.Warning("Another instance of vFalcon is already running!");
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

        Mutex?.ReleaseMutex();
        base.OnExit(e);
    }

    public static MainWindowViewModel GetMainWindowViewModel()
    {
        return Application.Current?.MainWindow?.DataContext as MainWindowViewModel;
    }
}
