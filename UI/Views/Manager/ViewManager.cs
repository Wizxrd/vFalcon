using System.Windows;
using vFalcon.UI.Views.Common;
using vFalcon.UI.Views.Toolbar;
using vFalcon.UI.Views.Tooolbar;
using vFalcon.Utils;
namespace vFalcon.UI.Views.Manager;

public class ViewManager
{
    public NewProfileView NewProfileView { get; set; }
    public PositionsView? PositionsView { get; set; }
    public MapsView? MapsView { get; set; }
    public FiltersView? FiltersView { get; set; }
    public FindView? FindView { get; set; }
    public GeneralSettingsView? GeneralSettingsView { get; set; }
    public AppearanceSettingsView? AppearanceSettingsView { get; set; }
    public AircraftListView? AircraftListView { get; set; }
    public SaveProfileAsView? SaveProfileAsView { get; set; }

    public void InitializeSettings()
    {
        if (App.Profile.PositionsSettings.WindowSettings.IsOpen) OpenPositionsView();
        if (App.Profile.MapSettings.WindowSettings.IsOpen) OpenMapsView();
        if (App.Profile.FilterSettings.WindowSettings.IsOpen) OpenFiltersView();
        if (App.Profile.FindSettings.WindowSettings.IsOpen) OpenFindView();
        if (App.Profile.GeneralSettings.WindowSettings.IsOpen) OpenGeneralSettingsView();
        if (App.Profile.AppearanceSettings.WindowSettings.IsOpen) OpenAppearanceSettingsView();
        if (App.Profile.AircraftListSettings.WindowSettings.IsOpen) OpenAircraftListView();
    }


    public void OpenPositionsView()
    {
        if (PositionsView != null) return;
        PositionsView = new();
        PositionsView.Owner = Application.Current.MainWindow;
        PositionsView.Closing += (_, __) =>
        {
            App.Profile.PositionsSettings.WindowSettings.IsOpen = false;
            PositionsView = null;
        };
        App.Profile.PositionsSettings.WindowSettings.IsOpen = true;
        PositionsView.Show();
    }

    public void OpenMapsView()
    {
        if (MapsView != null) return;
        MapsView = new();
        MapsView.Owner = Application.Current.MainWindow;
        MapsView.Closing += (_, __) =>
        {
            App.Profile.MapSettings.WindowSettings.IsOpen = false;
            MapsView = null; 
        };
        App.Profile.MapSettings.WindowSettings.IsOpen = true;
        MapsView.Show();
    }

    public void OpenFiltersView()
    {
        if (FiltersView != null) return;
        FiltersView = new();
        FiltersView.Owner = Application.Current.MainWindow;
        FiltersView.Closing += (_, __) =>
        {
            App.Profile.FilterSettings.WindowSettings.IsOpen = false;
            FiltersView = null;
        };
        App.Profile.FilterSettings.WindowSettings.IsOpen = true;
        FiltersView.Show();
    }

    public void OpenFindView()
    {
        if (FindView != null) return;
        FindView = new();
        FindView.Owner = Application.Current.MainWindow;
        FindView.Closing += (_, __) =>
        {
            App.Profile.FindSettings.WindowSettings.IsOpen = false;
            FindView = null;
        };
        App.Profile.FindSettings.WindowSettings.IsOpen = true;
        FindView.Show();
    }

    public void OpenGeneralSettingsView()
    {
        if (GeneralSettingsView != null) return;
        GeneralSettingsView = new();
        GeneralSettingsView.Owner = Application.Current.MainWindow;
        GeneralSettingsView.Closing += (_, __) =>
        {
            App.Profile.GeneralSettings.WindowSettings.IsOpen = false;
            GeneralSettingsView = null;
        };
        App.Profile.GeneralSettings.WindowSettings.IsOpen = true;
        GeneralSettingsView.Show();
    }

    public void OpenAppearanceSettingsView()
    {
        if (AppearanceSettingsView != null) return;
        AppearanceSettingsView = new();
        AppearanceSettingsView.Owner = Application.Current.MainWindow;
        AppearanceSettingsView.Closing += (_, __) =>
        {
            App.Profile.AppearanceSettings.WindowSettings.IsOpen = false;
            AppearanceSettingsView = null;
        };
        App.Profile.AppearanceSettings.WindowSettings.IsOpen = true;
        AppearanceSettingsView.Show();
    }

    public void OpenAircraftListView()
    {
        if (AircraftListView != null) return;
        AircraftListView = new();
        AircraftListView.Owner = Application.Current.MainWindow;
        AircraftListView.Closing += (_, __) =>
        {
            App.Profile.AircraftListSettings.WindowSettings.IsOpen = false;
            AircraftListView = null;
        };
        App.Profile.AircraftListSettings.WindowSettings.IsOpen = true;
        Logger.Debug("HE", "WAS HERE");
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

    public void OpenManagedArtccsView()
    {
        ManageArtccsView manageArtccsView = new ManageArtccsView();
        manageArtccsView.Owner = Application.Current.MainWindow;
        manageArtccsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        manageArtccsView.ShowDialog();
    }

    public void OpenNewProfileView()
    {
        NewProfileView = new NewProfileView();
        NewProfileView.Owner = Application.Current.MainWindow;
        NewProfileView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        NewProfileView.ShowDialog();
    }

    public void Dispose()
    {
        if (PositionsView != null) PositionsView.Close();
        if (MapsView != null) MapsView.Close();
        if (FiltersView != null) FiltersView.Close();
        if (FindView != null) FindView.Close();
        if (GeneralSettingsView != null) GeneralSettingsView.Close();
        if (AppearanceSettingsView != null) AppearanceSettingsView.Close();
        if (AircraftListView != null) AircraftListView.Close();
        if (SaveProfileAsView != null) SaveProfileAsView.Close();
    }
}
