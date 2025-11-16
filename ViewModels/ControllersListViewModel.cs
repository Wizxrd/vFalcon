using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using vFalcon.Commands;
using vFalcon.DataFeeds;
using vFalcon.Models;
using static vFalcon.ViewModels.AircraftListToolbarViewModel;

namespace vFalcon.ViewModels
{
    public class ControllersListViewModel : ViewModelBase
    {
        public class ControllerItem : ViewModelBase
        {
            private string facilityName = string.Empty;
            private ObservableCollection<string> radioNames = new ObservableCollection<string>();
            private ObservableCollection<string> radioFreqs = new ObservableCollection<string>();

            public string FacilityName
            {
                get => facilityName;
                set
                {
                    facilityName = value;
                    OnPropertyChanged();
                }
            }

            public ObservableCollection<string> RadioNames
            {
                get => radioNames;
                set { radioNames = value ?? new(); OnPropertyChanged(); }
            }

            public ObservableCollection<string> RadioFreqs
            {
                get => radioFreqs;
                set { radioFreqs = value ?? new(); OnPropertyChanged(); }
            }
        }
        public ObservableCollection<ControllerItem> Controllers { get; }
        public DispatcherTimer RefreshTimer = new();
        private EramViewModel eramViewModel;

        public ControllersListViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            Controllers = new ObservableCollection<ControllerItem>();
            RefreshControllersList(null, null);
            RefreshTimer.Interval = TimeSpan.FromSeconds(15);
            RefreshTimer.Tick += RefreshControllersList;
            RefreshTimer.Start();
        }

        public void StopRefreshTimer()
        {
            RefreshTimer.Tick -= RefreshControllersList;
            RefreshTimer.Stop();
            RefreshTimer = null;
        }

        private async void RefreshControllersList(object? sender, EventArgs? e)
        {
            JArray neighbors = await vNasDataFeed.GetArtccNeighboringPositions(
                eramViewModel.profile.FacilityId, eramViewModel.artcc.facility);

            Controllers.Clear();

            foreach (JObject neighbor in neighbors)
            {
                string facilityName = (string)neighbor["FacilityName"];
                ObservableCollection<string> names = new();
                ObservableCollection<string> freqs = new();

                foreach (string radioName in (JArray)neighbor["RadioNames"]) names.Add(radioName);
                foreach (string radioFreq in (JArray)neighbor["RadioFreqs"]) freqs.Add(radioFreq);

                Controllers.Add(new ControllerItem
                {
                    FacilityName = facilityName,
                    RadioNames = names,
                    RadioFreqs = freqs
                });
            }

            // After it's done: sort with ARTCC first, then alphabetical within groups
            var sorted = Controllers
                .OrderByDescending(c => (c.FacilityName?.IndexOf("ARTCC", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                .ThenBy(c => c.FacilityName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();

            Controllers.Clear();
            foreach (var item in sorted) Controllers.Add(item);
        }
    }
}
