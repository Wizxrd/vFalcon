using System.Windows;
using System.Windows.Input;
using vFalcon.Mvvm;
namespace vFalcon.UI.ViewModels.Toolbar;

public class FiltersViewModel : ViewModelBase
{
    private bool enabled = false;
    private bool requireAll = false;
    private string departure = string.Empty;
    private string arrival = string.Empty;
    private string sid = string.Empty;
    private string star = string.Empty;
    private string airline = string.Empty;
    private string altLow = string.Empty;
    private string altHigh = string.Empty;
    private Visibility saveFiltersVisibility = Visibility.Collapsed;

    public bool Enabled
    {
        get => enabled;
        set
        {
            enabled = value;
            SaveFiltersVisibility = Visibility.Visible;
            if (!value) App.MainWindowViewModel.PilotService.Refresh();
            OnPropertyChanged();
        }
    }
    public bool RequireAll
    {
        get => requireAll;
        set
        {
            requireAll = value;
            SaveFiltersVisibility = Visibility.Visible;
            OnPropertyChanged();
        }
    }
    public string Departure
    {
        get => departure;
        set
        {
            departure = value;
            OnPropertyChanged();
        }
    }
    public string Arrival
    {
        get => arrival;
        set
        {
            arrival = value;
            OnPropertyChanged();
        }
    }
    public string Sid
    {
        get => sid;
        set
        {
            sid = value;
            OnPropertyChanged();
        }
    }
    public string Star
    {
        get => star;
        set
        {
            star = value;
            OnPropertyChanged();
        }
    }
    public string Airline
    {
        get => airline;
        set
        {
            airline = value;
            OnPropertyChanged();
        }
    }
    public string AltLow
    {
        get => altLow;
        set
        {
            altLow = value;
            OnPropertyChanged();
        }
    }
    public string AltHigh
    {
        get => altHigh;
        set
        {
            altHigh = value;
            OnPropertyChanged();
        }
    }
    public Visibility SaveFiltersVisibility
    {
        get => saveFiltersVisibility;
        set
        {
            saveFiltersVisibility = value;
            OnPropertyChanged();
        }
    }

    public ICommand SaveFiltersCommand { get; set; }
    public FiltersViewModel()
    {
        Enabled = App.Profile.FilterSettings.Enabled;
        RequireAll = App.Profile.FilterSettings.RequireAll;
        Departure = App.Profile.FilterSettings.Departure;
        Arrival = App.Profile.FilterSettings.Arrival;
        Sid = App.Profile.FilterSettings.Sid;
        Star = App.Profile.FilterSettings.Star;
        Airline = App.Profile.FilterSettings.Airline;
        AltLow = App.Profile.FilterSettings.AltLow.ToString();
        AltHigh = App.Profile.FilterSettings.AltHigh.ToString();
        SaveFiltersCommand = new RelayCommand(OnSaveFiltersCommand);
        SaveFiltersVisibility = Visibility.Collapsed;
    }

    private void OnSaveFiltersCommand()
    {
        App.Profile.FilterSettings.Enabled = Enabled;
        App.Profile.FilterSettings.RequireAll = RequireAll;
        App.Profile.FilterSettings.Departure = Departure;
        App.Profile.FilterSettings.Arrival = Arrival;
        App.Profile.FilterSettings.Sid = Sid;
        App.Profile.FilterSettings.Star = Star;
        App.Profile.FilterSettings.Airline = Airline;
        App.Profile.FilterSettings.AltLow = int.TryParse(AltLow, out var low) ? low : 0;
        App.Profile.FilterSettings.AltHigh = int.TryParse(AltHigh, out var high) ? high : 0;
        App.MainWindowViewModel.GraphicsEngine.RequestRender();
        SaveFiltersVisibility = Visibility.Collapsed;
    }
}
