using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Utils;

namespace vFalcon.UI.ViewModels.Controls;

public class PilotContextViewModel : ViewModelBase
{
    private Pilot pilot;
    private int datablockType;
    private bool driEnabled = false;
    private int driSize;
    private int datablockPosition;
    private int leaderLineLength;

    private string time = string.Empty;
    private string pilotCallsign = string.Empty;
    private string cid = string.Empty;
    private string type = string.Empty;
    private string reportedAltitude = string.Empty;
    private string requestedAltitude = string.Empty;
    private string heading = string.Empty;
    private string speed = string.Empty;
    private string assignedBeacon = string.Empty;
    private string frequency = string.Empty;
    private string flightRules = string.Empty;
    private string latLon = string.Empty;
    private string flightPlan = string.Empty;

    public Pilot Pilot
    {
        get => pilot;
        set
        {
            pilot = value;
        }
    }
    public int DatablockType
    {
        get => datablockType;
        set
        {
            datablockType = value;
            pilot.DatablockType = (DatablockType)value;
            Pilot.ForcedDatablockType = true;
            OnPropertyChanged();
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }
    public bool DriEnabled
    {
        get => driEnabled;
        set
        {
            driEnabled = value;
            Pilot.DriEnabled = value;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            OnPropertyChanged();
        }
    }
    public int DriSize
    {
        get => driSize;
        set
        {
            if (driSize == value) return;
            driSize = value;
            Pilot.DriSize = value;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            OnPropertyChanged();
        }
    }
    public int DatablockPosition
    {
        get => datablockPosition;
        set
        {
            int clamped = Math.Max(1, Math.Min(9, value));
            if (datablockPosition == clamped) return;
            datablockPosition = clamped;
            Pilot.DatablockPosition = (Utils.DatablockPosition)clamped;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            OnPropertyChanged();
        }
    }
    public int LeaderLineLength
    {
        get => leaderLineLength;
        set
        {
            int clamped = Math.Max(0, Math.Min(3, value));
            if (leaderLineLength == clamped) return;
            leaderLineLength = clamped;
            Pilot.LeaderLineLength = clamped;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            OnPropertyChanged();
        }
    }
    public string Time
    {
        get => time;
        set
        {
            time = value;
            OnPropertyChanged();
        }
    }
    public string PilotCallsign
    {
        get => pilotCallsign;
        set
        {
            pilotCallsign = value;
            OnPropertyChanged();
        }
    }
    public string CID
    {
        get => cid;
        set
        {
            cid = value;
            OnPropertyChanged();
        }
    }
    public string Type
    {
        get => type;
        set
        {
            type = value;
            OnPropertyChanged();
        }
    }
    public string ReportedAltitude
    {
        get => reportedAltitude;
        set
        {
            reportedAltitude = value;
            OnPropertyChanged();
        }
    }
    public string RequestedAltitude
    {
        get => requestedAltitude;
        set
        {
            requestedAltitude = value;
            OnPropertyChanged();
        }
    }
    public string Heading
    {
        get => heading;
        set
        {
            heading = value;
            OnPropertyChanged();
        }
    }
    public string Speed
    {
        get => speed;
        set
        {
            speed = value;
            OnPropertyChanged();
        }
    }
    public string AssignedBeacon
    {
        get => assignedBeacon;
        set
        {
            assignedBeacon = value;
            OnPropertyChanged();
        }
    }
    public string Frequency
    {
        get => frequency;
        set
        {
            frequency = value;
            OnPropertyChanged();
        }

    }
    public string FlightRules
    {
        get => flightRules;
        set
        {
            flightRules = value;
            OnPropertyChanged();
        }
    }
    public string LatLon
    {
        get => latLon;
        set
        {
            latLon = value;
            OnPropertyChanged();
        }
    }
    public string FlightPlan
    {
        get => flightPlan;
        set
        {
            flightPlan = value;
            OnPropertyChanged();
        }
    }

    public ICommand ToggleFullDatablockCommand { get; set; }
    public ICommand ToggleDwellLockCommand { get; set; }
    public ICommand CopyCallsignCommand { get; set; }
    public ICommand CopyCIDCommand { get; set; }
    public ICommand CopyLatLonCommand { get; set; }
    public ICommand DisplayFiledRouteCommand { get; set; }
    public ICommand DisplayFullRouteCommand { get; set; }
    public ICommand PlotOnSkyVectorCommand { get; set; }
    public ICommand CopyFlightPlanCommand { get; set; }

    public PilotContextViewModel()
    {
        ToggleFullDatablockCommand = new RelayCommand(OnToggleFullDatablockCommand);
        ToggleDwellLockCommand = new RelayCommand(OnToggleDwellLockCommand);
        CopyCallsignCommand = new RelayCommand(OnCopyCallsignCommand);
        CopyCIDCommand = new RelayCommand(OnCopyCIDCommand);
        CopyLatLonCommand = new RelayCommand(OnCopyLatLonCommand);
        DisplayFiledRouteCommand = new RelayCommand(OnDisplayFiledRouteCommand);
        DisplayFullRouteCommand = new RelayCommand(OnDisplayFullRouteCommand);
        PlotOnSkyVectorCommand = new RelayCommand(OnPlotOnSkyVectorCommand);
        CopyFlightPlanCommand = new RelayCommand(OnCopyFlightPlanCommand);
    }

    private void OnToggleFullDatablockCommand()
    {
        Pilot.ForcedFullDatablock = !Pilot.ForcedFullDatablock;
        Pilot.FullDatablockEnabled = !Pilot.FullDatablockEnabled;
        App.MainWindowViewModel.GraphicsEngine.RequestRender();
    }

    private void OnToggleDwellLockCommand()
    {
        Pilot.DwellEmphasisEnabled = !Pilot.DwellEmphasisEnabled;
        App.MainWindowViewModel.GraphicsEngine.RequestRender();
    }

    private void OnCopyCallsignCommand()
    {
        Clipboard.SetText(pilot.Callsign);
    }

    private void OnCopyCIDCommand()
    {
        Clipboard.SetText(pilot.CID.ToString());
    }

    private void OnCopyLatLonCommand()
    {
        Clipboard.SetText($"{Pilot.Latitude}, {Pilot.Longitude}");
    }

    private void OnDisplayFiledRouteCommand()
    {
        Pilot.DisplayFiledRoute = !Pilot.DisplayFiledRoute;
        App.MainWindowViewModel.GraphicsEngine.RequestRender();
    }

    private void OnDisplayFullRouteCommand()
    {
        Pilot.DisplayFullRoute = !Pilot.DisplayFullRoute;
        App.MainWindowViewModel.GraphicsEngine.RequestRender();
    }

    private void OnPlotOnSkyVectorCommand()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = $"https://skyvector.com/?ll=42.780888072479314,-98.85498046372577&chart=301&zoom=13&fpl=%20{Pilot.FlightPlan["departure"]}%20{Pilot.FlightPlan["route"]}%20{Pilot.FlightPlan["arrival"]}",
            UseShellExecute = true
        });
    }

    private void OnCopyFlightPlanCommand()
    {
        string route = Regex.Replace(FlightPlan ?? string.Empty, @"\./\.|\.", " ");
        route = Regex.Replace(route, @"\s+", " ").Trim();
        Clipboard.SetText(route);
    }
}
