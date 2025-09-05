using AdonisUI.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class PilotContextViewModel : ViewModelBase
    {
        private string pilotCallsign = string.Empty;
        private bool jRingEnabled;
        private int fullDataBlockPosition;
        private int leaderLingLength;
        private Pilot? pilot;
        private RadarViewModel? radarViewModel;

        public string PilotCallsign
        {
            get => pilotCallsign;
            set { pilotCallsign = value; OnPropertyChanged(); }
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
            set { radarViewModel = value; OnPropertyChanged(); }
        }

        public ICommand DisplayRouteCommand { get; set; }

        public PilotContextViewModel()
        {
            DisplayRouteCommand = new RelayCommand(OnDisplayRouteCommand);
        }

        private void OnDisplayRouteCommand()
        {

        }
    }
}
