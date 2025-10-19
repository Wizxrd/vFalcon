using AdonisUI.Controls;
using AdonisUI.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class PilotContextViewModel : ViewModelBase
    {
        private string time = string.Empty;
        private string pilotCallsign = string.Empty;
        private string cid = string.Empty;
        private string type = string.Empty;
        private string wakeCategory = string.Empty;
        private string reportedAltitude = string.Empty;
        private string requestedAltitude = string.Empty;
        private string heading = string.Empty;
        private string speed = string.Empty;
        private string assignedBeacon = string.Empty;
        private string flightRules = string.Empty;
        private string latLon = string.Empty;
        private string searchByCallsignText = string.Empty;
        private string searchByFixText = string.Empty;
        private string searchByDepartureText = string.Empty;
        private string searchByArrivalText = string.Empty;
        private string flightPlan = string.Empty;

        private bool jRingEnabled;
        private int fullDataBlockPosition;
        private int leaderLingLength;
        private Pilot? pilot;
        private RadarViewModel? radarViewModel;
        private Regex _routeSplitRegex = new(@"\s+", RegexOptions.Compiled);

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
            set {
                pilotCallsign = value;
                OnPropertyChanged(); }
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
        public string WakeCategory
        {
            get => wakeCategory;
            set
            {
                wakeCategory = value;
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

        public string SearchByCallsignText
        {
            get => searchByCallsignText;
            set
            {
                searchByCallsignText = value;
                OnPropertyChanged();
            }
        }

        public string SearchByFixText
        {
            get => searchByFixText;
            set
            {
                searchByFixText = value;
                OnPropertyChanged();
            }
        }

        public string SearchByDepartureText
        {
            get => searchByDepartureText;
            set
            {
                searchByDepartureText = value;
                OnPropertyChanged();
            }
        }

        public string SearchByArrivalText
        {
            get => searchByArrivalText;
            set
            {
                searchByArrivalText = value;
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

        public bool JRingEnabled
        {
            get => jRingEnabled;
            set
            {
                if (jRingEnabled == value) return;
                jRingEnabled = value;
                if (pilot != null) pilot.JRingEnabled = value;
                radarViewModel?.Redraw();
                OnPropertyChanged();
            }
        }

        private int jRingSize;
        public int JRingSize
        {
            get => jRingSize;
            set
            {
                if (jRingSize == value) return;
                jRingSize = value;
                if (pilot != null) pilot.JRingSize = value;
                radarViewModel?.Redraw();
                OnPropertyChanged();
            }
        }

        public int FullDataBlockPosition
        {
            get => fullDataBlockPosition;
            set
            {
                int clamped = Math.Max(1, Math.Min(9, value));
                if (fullDataBlockPosition == clamped) return;
                fullDataBlockPosition = clamped;
                if (pilot != null) pilot.FullDataBlockPosition = clamped;
                radarViewModel?.Redraw();
                OnPropertyChanged();
            }
        }

        public int LeaderLineLength
        {
            get => leaderLingLength;
            set
            {
                int clamped = Math.Max(0, Math.Min(3, value));
                if (leaderLingLength == clamped) return;
                leaderLingLength = clamped;
                if (pilot != null) pilot.LeaderLingLength = clamped;
                radarViewModel?.Redraw();
                OnPropertyChanged();
            }
        }

        public Pilot? Pilot
        {
            get => pilot;
            set { pilot = value; OnPropertyChanged(); }
        }

        public RadarViewModel? RadarViewModel
        {
            get => radarViewModel;
            set { 
                radarViewModel = value;
                OnPropertyChanged(); }
        }

        private int datablockType;
        public int DatablockType
        {
            get => datablockType;
            set
            {
                datablockType = value;
                if (pilot == null) return;
                if (value == 0) pilot.DatablockType = "ERAM";
                else if (value == 1) pilot.DatablockType = "STARS";
                radarViewModel?.Redraw();
                OnPropertyChanged();
            }
        }

        public ICommand DisplayRouteCommand { get; set; }
        public ICommand CopyFlightPlanCommand { get; set; }
        public ICommand CopyCallsignCommand { get; set; }
        public ICommand CopyCIDCommand { get; set; }
        public ICommand CopyLatLonCommand { get; set; }
        public ICommand PlotOnSkyVectorCommand {  get; set; }
        public ICommand ToggleFullDatablockCommand { get; set; }
        public ICommand ToggleDwellLockCommand { get; set; }
        public ICommand SearchByCallsignCommand { get; set; }
        public ICommand SearchByFixCommand { get; set; }
        public ICommand SearchByDepartureCommand { get; set; }
        public ICommand SearchByArrivalCommand {  get; set; }
        public ICommand ClearSearchesCommand { get; set; }

        public ICommand DisplayFullRouteCommand { get; set; }

        private Visibility fullRouteVisibility = Visibility.Collapsed;
        public Visibility FullRouteVisibility
        {
            get => fullRouteVisibility;
            set
            {
                fullRouteVisibility = value;
                OnPropertyChanged();
            }
        }

        public PilotContextViewModel()
        {
            ToggleFullDatablockCommand = new RelayCommand(OnToggleFullDatablockCommand);
            ToggleDwellLockCommand = new RelayCommand(OnToggleDwellLockCommand);
            DisplayRouteCommand = new RelayCommand(OnDisplayRouteCommand);
            CopyFlightPlanCommand = new RelayCommand(OnCopyFlightPlanCommand);
            CopyCallsignCommand = new RelayCommand(OnCopyCallsignCommand);
            CopyCIDCommand = new RelayCommand(OnCopyCIDCommand);
            CopyLatLonCommand = new RelayCommand(OnCopyLatLonCommand);
            PlotOnSkyVectorCommand = new RelayCommand(OnPlotOnSkyVectorCommand);
            SearchByCallsignCommand = new RelayCommand(OnSearchByCallsignCommand);
            SearchByFixCommand = new RelayCommand(OnSearchByFixCommand);
            SearchByDepartureCommand = new RelayCommand(OnSearchByDepartureCommand);
            SearchByArrivalCommand = new RelayCommand(OnSearchByArrivalCommand);
            ClearSearchesCommand = new RelayCommand(OnClearSearchesCommand);
            DisplayFullRouteCommand = new RelayCommand(OnDisplayFullRouteCommand);
        }

        private void OnSearchByCallsignCommand()
        {
            var pilots = radarViewModel?.pilotService.Pilots.Values.ToList() ?? new List<Pilot>();
            foreach (Pilot pilot in pilots)
            {
                if (string.IsNullOrEmpty(searchByCallsignText) || string.IsNullOrEmpty(searchByCallsignText)) return;
                if (pilot.Callsign == searchByCallsignText)
                {
                    pilot.DwellLock = true;
                    radarViewModel?.Redraw();
                    return;
                }
            }
        }

        private void OnSearchByFixCommand()
        {
            var pilots = radarViewModel?.pilotService.Pilots.Values.ToList() ?? new List<Pilot>();
            foreach (Pilot pilot in pilots)
            {
                if (string.IsNullOrEmpty(searchByFixText) || string.IsNullOrEmpty(searchByFixText)) return;
                string route = (string)pilot.FlightPlan?["route"] ?? string.Empty;
                if (string.IsNullOrEmpty(route) || string.IsNullOrEmpty(route)) continue;
                var tokens = _routeSplitRegex.Split(route);

                for (int i = 0; i < tokens.Length; i++)
                {
                    var t = tokens[i];
                    if (t == searchByFixText)
                    {
                        pilot.DwellLock = true;
                    }
                }
            }
            radarViewModel?.Redraw();
        }

        private void OnSearchByDepartureCommand()
        {
            var pilots = radarViewModel?.pilotService.Pilots.Values.ToList() ?? new List<Pilot>();
            foreach (Pilot pilot in pilots)
            {
                if (string.IsNullOrEmpty(searchByDepartureText) || string.IsNullOrEmpty(searchByDepartureText)) return;
                if ((string)pilot.FlightPlan?["departure"] == searchByDepartureText)
                {
                    pilot.DwellLock = true;
                }
            }
            radarViewModel?.Redraw();
        }

        private void OnSearchByArrivalCommand()
        {
            var pilots = radarViewModel?.pilotService.Pilots.Values.ToList() ?? new List<Pilot>();
            foreach (Pilot pilot in pilots)
            {
                if (string.IsNullOrEmpty(searchByArrivalText) || string.IsNullOrEmpty(searchByArrivalText)) return;
                if ((string)pilot.FlightPlan?["arrival"] == searchByArrivalText)
                {
                    pilot.DwellLock = true;
                }
            }
            radarViewModel?.Redraw();
        }

        private void OnClearSearchesCommand()
        {
            SearchByCallsignText = string.Empty;
            SearchByFixText = string.Empty;
            SearchByDepartureText = string.Empty;
            SearchByArrivalText = string.Empty;
        }

        private void OnToggleFullDatablockCommand()
        {
            pilot.ForcedFullDataBlock = !pilot.ForcedFullDataBlock;
            pilot.FullDataBlock = !pilot.FullDataBlock;
            radarViewModel?.Redraw();
        }

        private void OnToggleDwellLockCommand()
        {
            pilot.DwellLock = !pilot.DwellLock;
            radarViewModel?.Redraw();
        }

        private void OnCopyCallsignCommand()
        {
            Clipboard.SetText(pilot.Callsign);
        }

        private void OnCopyCIDCommand()
        {
            Clipboard.SetText(pilot.CID.ToString());
        }


        private void OnDisplayFullRouteCommand()
        {
            if (Pilot.DisplayRoute)
            {
                Pilot.DisplayRoute = false;
                Pilot.DisplayCoords = null;
            }
            else
            {
                Pilot.DisplayRoute = true;
                var coords = new JArray();
                var t = radarViewModel?.eramViewModel.playbackService.replayJson["Pilots"] as JObject;
                var pil = t[Pilot.Callsign];
                var hist = (JArray)pil["History"];
                for (int i = 0; i < hist.Count; i++)
                {
                    if (hist[i] is JArray pt && pt.Count >= 2)
                    {
                        double lat = pt[0]!.Value<double>(); 
                        double lon = pt[1]!.Value<double>();
                        coords.Add(new JArray(lat, lon));
                    }
                }
                Pilot.DisplayCoords = coords;
            }
            radarViewModel?.Redraw();
        }

        public void OnDisplayRouteCommand()
        {
            if (Pilot.DisplayRoute)
            {
                Pilot.DisplayRoute = false;
                Pilot.DisplayCoords = null;
            }
            else
            {
                Pilot.DisplayRoute = true;
            }
            radarViewModel?.Redraw();
        }

        private void OnCopyFlightPlanCommand()
        {
            string route = Regex.Replace(FlightPlan ?? string.Empty, @"\./\.|\.", " ");
            route = Regex.Replace(route, @"\s+", " ").Trim();
            Clipboard.SetText(route);
        }

        private void OnCopyLatLonCommand()
        {
            Clipboard.SetText($"{pilot.Latitude}, {pilot.Longitude}");
        }

        private void OnPlotOnSkyVectorCommand()
        {
            //https://skyvector.com/?ll=41.26899899509328,-87.77343749427645&chart=301&zoom=8&fpl=%20KDTW%20PAVYL3%20MRDOC%20HOXIE%20Q476%20WLKES%20KJFK
            string url = $"https://skyvector.com/?ll=39.74323907349597,-107.38183593301387&chart=301&zoom=20&fpl=%20{pilot.FlightPlan["departure"]}%20{pilot.FlightPlan["route"]}%20{pilot.FlightPlan["arrival"]}";
            url = Regex.Replace(url, @"\s+", "%20").Trim();
            Process.Start(new ProcessStartInfo
            {
                FileName = $"https://skyvector.com/?ll=41.26899899509328,-87.77343749427645&chart=301&zoom=8&fpl=%20{pilot.FlightPlan["departure"]}%20{pilot.FlightPlan["route"]}%20{pilot.FlightPlan["arrival"]}",
                UseShellExecute = true
            });
        }
    }
}
