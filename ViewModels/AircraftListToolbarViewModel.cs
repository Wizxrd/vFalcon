using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class AircraftListToolbarViewModel : ViewModelBase
    {
        private readonly EramViewModel eramViewModel;
        public DispatcherTimer RefreshTimer = new();
        private string _searchQuery;
        private bool searchFiltering = false;

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    if (!string.IsNullOrWhiteSpace(value)) searchFiltering = true;
                    else
                    {
                        searchFiltering = false;
                    }
                    _searchQuery = value;
                    OnPropertyChanged();
                    FilterAircraft();
                }
            }
        }

        private void FilterAircraft()
        {
            Pilots.Clear();
            string query = SearchQuery?.ToLower() ?? string.Empty;
            foreach (Pilot pilot in eramViewModel.RadarViewModel.pilotService.Pilots.Values.ToList())
            {
                if (string.IsNullOrWhiteSpace(query) || pilot.Callsign.ToLower().Contains(query))
                {
                    PilotItem pilotItem = new PilotItem
                    {
                        Callsign = pilot.Callsign,
                        IsChecked = pilot.DisplayRoute
                    };
                    pilotItem.Command = new RelayCommand(_ =>
                    {
                        if (pilotItem.IsChecked)
                        {
                            pilot.DisplayRoute = true;
                        }
                        else
                        {
                            pilot.DisplayRoute = false;
                            pilot.DisplayCoords = null;
                        }
                        eramViewModel.RadarViewModel.Redraw();
                    });
                    Pilots.Add(pilotItem);
                }
            }
        }

        public ObservableCollection<PilotItem> Pilots { get; }

        public class PilotItem : ViewModelBase
        {
            private string callsign;
            private bool isChecked;
            public string Callsign
            {
                get => callsign;
                set
                {
                    callsign = value;
                    OnPropertyChanged();
                }
            }

            public bool IsChecked
            {
                get => isChecked;
                set
                {
                    isChecked = value;
                    OnPropertyChanged();
                }
            }
            public ICommand Command { get; set; }
        }

        public AircraftListToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            Pilots = new ObservableCollection<PilotItem>();
            RefreshAircraftList(null, null);
            RefreshTimer.Interval = TimeSpan.FromSeconds(15);
            RefreshTimer.Tick += RefreshAircraftList;
            RefreshTimer.Start();
        }

        public void StopRefreshTimer()
        {
            RefreshTimer.Tick -= RefreshAircraftList;
            RefreshTimer.Stop();
            RefreshTimer = null;
        }

        private void RefreshAircraftList(object? sender, EventArgs? e)
        {
            if (searchFiltering == true) return;
            Pilots.Clear();
            if (eramViewModel.RadarViewModel.pilotService == null) return;
            foreach (Pilot pilot in eramViewModel.RadarViewModel.pilotService.Pilots.Values.ToList())
            {
                PilotItem pilotItem = new PilotItem
                {
                    Callsign = pilot.Callsign,
                    IsChecked = pilot.DisplayRoute
                };
                pilotItem.Command = new RelayCommand(_ =>
                {
                    if (pilotItem.IsChecked)
                    {
                        pilot.DisplayRoute = true;
                    }
                    else
                    {
                        pilot.DisplayRoute = false;
                        pilot.DisplayCoords = null;
                    }
                    eramViewModel.RadarViewModel.Redraw();
                });
                Pilots.Add(pilotItem);
            }
        }
    }
}
