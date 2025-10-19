using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Models;
using vFalcon.Services.Service;

namespace vFalcon.ViewModels
{
    class SearchOptionsToolbarViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;
        private string searchText = string.Empty;
        private bool searchButtonEnabled = false;

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value.ToUpper();
                SearchButtonEnabled = !string.IsNullOrEmpty(searchText);
                OnPropertyChanged();
            }
        }

        public bool SearchButtonEnabled
        {
            get => searchButtonEnabled;
            set
            {
                searchButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; set; }
        public ICommand ClearSearchCommand { get; set; }
        public SearchOptionsToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            SearchCommand = new RelayCommand(OnSearchCommand);
            ClearSearchCommand = new RelayCommand(OnClearSearchCommand);
        }

        private void OnSearchCommand()
        {
            if (searchText == string.Empty) return;
            var pilots = eramViewModel.RadarViewModel.pilotService.Pilots.Values.ToList() ?? new List<Pilot>();
            foreach (Pilot pilot in pilots)
            {
                string departure = (string)pilot.FlightPlan?["departure"];
                string arrival = (string)pilot.FlightPlan?["arrival"];
                if (searchText == pilot.Callsign)
                {
                    pilot.FullDataBlock = true;
                    pilot.ForcedFullDataBlock = true;
                    pilot.DwellLock = true;
                }
                if (searchText == departure)
                {
                    pilot.FullDataBlock = true;
                    pilot.ForcedFullDataBlock = true;
                    pilot.DwellLock = true;
                }
                if (searchText == arrival)
                {
                    pilot.FullDataBlock = true;
                    pilot.ForcedFullDataBlock = true;
                    pilot.DwellLock = true;
                }
            }
            SearchText = string.Empty;
            eramViewModel.RadarViewModel.Redraw();
        }

        private void OnClearSearchCommand()
        {
            SearchText = string.Empty;
        }
    }
}
