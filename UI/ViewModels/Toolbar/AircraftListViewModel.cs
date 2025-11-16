using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Utils;

namespace vFalcon.UI.ViewModels.Toolbar;

public class AircraftListViewModel : ViewModelBase
{
    public ScheduledFunction Scheduler;
    private string searchQuery = string.Empty;
    private bool searchFiltering = false;
    public ObservableCollection<PilotItem> Pilots { get; } = new();
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

    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            if (searchQuery != value)
            {
                if (!string.IsNullOrWhiteSpace(value)) searchFiltering = true;
                else
                {
                    searchFiltering = false;
                }
                searchQuery = value;
                OnPropertyChanged();
                FilterAircraft();
            }
        }
    }

    public AircraftListViewModel()
    {
        Scheduler = new(RefreshAircraftList, 15, true);
        Scheduler.Start();
    }

    private void FilterAircraft()
    {
        Pilots.Clear();
        string query = SearchQuery?.ToLower() ?? string.Empty;
        foreach (Pilot pilot in App.MainWindowViewModel.PilotService.Pilots.Values.ToList())
        {
            if (pilot.GroundSpeed < 30) continue;
            if (string.IsNullOrWhiteSpace(query) || pilot.Callsign.ToLower().Contains(query))
            {
                AddPilot(pilot);
            }
        }
    }

    private async Task RefreshAircraftList()
    {
        if (searchFiltering == true) return;
        Pilots.Clear();
        if (App.MainWindowViewModel.PilotService == null) return;
        foreach (Pilot pilot in App.MainWindowViewModel.PilotService.Pilots.Values.ToList())
        {
            AddPilot(pilot);
        }
    }

    private void AddPilot(Pilot pilot)
    {
        PilotItem pilotItem = new PilotItem
        {
            Callsign = pilot.Callsign,
            IsChecked = pilot.DisplayFiledRoute
        };
        pilotItem.Command = new RelayCommand(_ =>
        {
            if (pilotItem.IsChecked)
            {
                pilot.ForcedFullDatablock = true;
                pilot.FullDatablockEnabled = true;
            }
            else
            {
                pilot.ForcedFullDatablock = false;
                pilot.FullDatablockEnabled = false;
            }
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        });
        Pilots.Add(pilotItem);
    }
}
