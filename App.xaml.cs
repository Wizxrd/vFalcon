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
    public static string Version { get; } = "1.0.0";
    public static Profile Profile { get; set; }
    public static Artcc Artcc { get; set; }
    public static ViewManager ViewManager { get; set; } = new();
    public static MainWindowView MainWindowView { get; set; }
    public static MainWindowViewModel MainWindowViewModel { get; set; }

    public App()
    {
        Logger.DebugMode = true;
        Logger.LogLevelThreshold = LogLevel.Trace;
        Logger.Info("App", $"Launching vFalcon v{Version}");
    }

    public static MainWindowViewModel GetMainWindowViewModel()
    {
        return Application.Current?.MainWindow?.DataContext as MainWindowViewModel;
    }

}
