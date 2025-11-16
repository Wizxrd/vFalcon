using System.Windows;
using vFalcon.UI.Views.Common;
using vFalcon.UI.Views.Toolbar;
using vFalcon.UI.Views.Tooolbar;
namespace vFalcon.UI.Views.Manager;

public class ViewManager
{
    public PositionsView? PositionsView { get; set; }
    public MapsView? MapsView { get; set; }
    public FiltersView? FiltersView { get; set; }
    public FindView? FindView { get; set; }
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

    public void OpenFiltersView()
    {
        if (FiltersView != null) return;
        FiltersView = new();
        FiltersView.Owner = Application.Current.MainWindow;
        FiltersView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        FiltersView.Closing += (_, __) => FiltersView = null;
        FiltersView.Show();
    }

    public void OpenFindView()
    {
        if (FindView != null) return;
        FindView = new();
        FindView.Owner = Application.Current.MainWindow;
        FindView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        FindView.Closing += (_, __) => FindView = null;
        FindView.Show();
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

    public void OpenManagedArtccsView()
    {
        ManageArtccsView manageArtccsView = new ManageArtccsView();
        manageArtccsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        manageArtccsView.ShowDialog();
    }

    public void OpenNewProfileView()
    {
        NewProfileView newProfileView = new NewProfileView();
        newProfileView.Owner = Application.Current.MainWindow;
        newProfileView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        newProfileView.ShowDialog();
    }

    public void Dispose()
    {
        if (PositionsView != null) PositionsView.Close();
        if (MapsView != null) MapsView.Close();
        if (FiltersView != null) FiltersView.Close();
        if (GeneralSettingsView != null) GeneralSettingsView.Close();
        if (AppearanceSettingsView != null) AppearanceSettingsView.Close();
        if (AircraftListView != null) AircraftListView.Close();
        if (SaveProfileAsView != null) SaveProfileAsView.Close();
    }
}
