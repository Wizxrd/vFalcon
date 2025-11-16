using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Models;
using vFalcon.Services.Service;

namespace vFalcon.ViewModels
{
    class FindOptionsToolbarViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;
        private string searchText = string.Empty;
        private bool searchButtonEnabled = false;

        public string SearchText
        {
            get => searchText;
            set
            {
                if (value.Length > 5) return;
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

        public FindOptionsToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            SearchCommand = new RelayCommand(OnSearchCommand);
            ClearSearchCommand = new RelayCommand(OnClearSearchCommand);
        }

        private void OnSearchCommand()
        {
            if (searchText == string.Empty) return;
            List<double>? airportCoords = eramViewModel.RadarViewModel.routeService.GetAirportCoords(searchText);
            List<double>? fixCoords = eramViewModel.RadarViewModel.routeService.GetFixCoords(searchText);
            if (airportCoords != null)
            {
                eramViewModel.RadarViewModel.Find = true;
                eramViewModel.RadarViewModel.FindCoords = airportCoords;
                eramViewModel.RadarViewModel.Redraw();
                SearchText = string.Empty;
            }
            else if (fixCoords != null)
            {
                eramViewModel.RadarViewModel.Find = true;
                eramViewModel.RadarViewModel.FindCoords = fixCoords;
                eramViewModel.RadarViewModel.Redraw();
                SearchText = string.Empty;
            }
        }

        private void OnClearSearchCommand()
        {
            SearchText = string.Empty;
            eramViewModel.RadarViewModel.Find = false;
            eramViewModel.RadarViewModel.FindCoords = null;
            eramViewModel.RadarViewModel.Redraw();
        }
    }
}
