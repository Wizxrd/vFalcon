using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using vFalcon.UI.Views.Common;
using vFalcon.UI.Views.Toolbar;
using vFalcon.UI.Views.Tooolbar;
namespace vFalcon.UI.Views.Manager;

public class ViewManager
{
    public PositionsView? PositionsView { get; set; }
    public MapsView? MapsView { get; set; }
    public GeneralSettingsView? GeneralSettingsView { get; set; }
    public AppearanceSettingsView? AppearanceSettingsView { get; set; }
    public AircraftListView? AircraftListView { get; set; }
    public SaveProfileAsView? SaveProfileAsView { get; set; }

    public void OpenPositionsView()
    {
        if (PositionsView != null) return;
        PositionsView = new();
        PositionsView.Owner = Application.Current.MainWindow;
        PositionsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        PositionsView.Closing += (_, __) => PositionsView = null;
        PositionsView.Show();
    }

    public void OpenMapsView()
    {
        if (MapsView != null) return;
        MapsView = new();
        MapsView.Owner = Application.Current.MainWindow;
        MapsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        MapsView.Closing += (_, __) => MapsView = null;
        MapsView.Show();
    }

    public void OpenGeneralSettingsView()
    {
        if (GeneralSettingsView != null) return;
        GeneralSettingsView = new();
        GeneralSettingsView.Owner = Application.Current.MainWindow;
        GeneralSettingsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        GeneralSettingsView.Closing += (_, __) => GeneralSettingsView = null;
        GeneralSettingsView.Show();
    }

    public void OpenAppearanceSettingsView()
    {
        if (AppearanceSettingsView != null) return;
        AppearanceSettingsView = new();
        AppearanceSettingsView.Owner = Application.Current.MainWindow;
        AppearanceSettingsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        AppearanceSettingsView.Closing += (_, __) => AppearanceSettingsView = null;
        AppearanceSettingsView.Show();
    }

    public void OpenAircraftListView()
    {
        if (AircraftListView != null) return;
        AircraftListView = new();
        AircraftListView.Owner = Application.Current.MainWindow;
        AircraftListView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        AircraftListView.Closing += (_, __) => AircraftListView = null;
        AircraftListView.Show();
    }

    public void OpenSaveProfileAsView()
    {
        if (SaveProfileAsView != null) return;
        SaveProfileAsView = new();
        SaveProfileAsView.Owner = Application.Current.MainWindow;
        SaveProfileAsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SaveProfileAsView.Closing += (_, __) => SaveProfileAsView = null;
        SaveProfileAsView.ShowDialog();
    }

    public void OpenLoadProfileView()
    {
        LoadProfileView loadProfileView = new();
        Application.Current.MainWindow = loadProfileView;
        App.MainWindowView.Close();
        loadProfileView.ShowDialog();
    }

    public void Dispose()
    {
        if (PositionsView != null) PositionsView.Close();
        if (MapsView != null) MapsView.Close();
        if (GeneralSettingsView != null) GeneralSettingsView.Close();
        if (AppearanceSettingsView != null) AppearanceSettingsView.Close();
        if (AircraftListView != null) AircraftListView.Close();
        if (SaveProfileAsView != null) SaveProfileAsView.Close();
    }
}
