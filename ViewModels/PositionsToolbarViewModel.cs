using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Service;

namespace vFalcon.ViewModels
{
    public class PositionsToolbarViewModel : ViewModelBase
    {
        private readonly EramViewModel eramViewModel;
        private readonly Artcc artcc;
        private readonly Profile profile;

        public class SectorItem : ViewModelBase
        {
            private string sectorId;
            private bool isChecked;

            public string SectorId
            {
                get => sectorId;
                set
                {
                    sectorId = value;
                    OnPropertyChanged();
                }
            }

            public bool IsChecked
            {
                get => isChecked;
                set
                {
                    if (isChecked != value)
                    {
                        isChecked = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public ObservableCollection<SectorItem> ArtccSectors { get; }
        public PositionsToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            artcc = eramViewModel.artcc;
            profile = eramViewModel.profile;
            ArtccSectors = new ObservableCollection<SectorItem>();
            if (eramViewModel.profile.DisplayType == "ERAM")
            {
                foreach (var sector in ArtccService.GetArtccSectors(profile.ArtccId))
                {
                    int lastDash = sector.LastIndexOf('-');
                    string sectorName = sector.Substring(0, lastDash).Trim();
                    bool isChecked = false;
                    if (eramViewModel.ActivatedSectors.ContainsKey(sectorName))
                    {
                        isChecked = true;
                    }
                    SectorItem item = new SectorItem
                    {
                        SectorId = sector,
                        IsChecked = isChecked
                    };
                    item.PropertyChanged += SectorPropertyChanged;
                    ArtccSectors.Add(item);
                }
            }
            else if (eramViewModel.profile.DisplayType == "STARS")
            {
                foreach (var sector in ArtccService.GetStarsPositions(eramViewModel))
                {
                    int lastDash = sector.LastIndexOf('-');
                    string sectorName = sector.Substring(0, lastDash).Trim();
                    bool isChecked = false;
                    if (eramViewModel.ActivatedSectors.ContainsKey(sectorName))
                    {
                        isChecked = true;
                    }
                    SectorItem item = new SectorItem
                    {
                        SectorId = sector,
                        IsChecked = isChecked
                    };
                    item.PropertyChanged += SectorPropertyChanged;
                    ArtccSectors.Add(item);
                }
            }
        }

        private void SectorPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SectorItem.IsChecked)) return;
            if (sender is not SectorItem sectorItem) return;

            var id = sectorItem.SectorId;
            int lastDash = id.LastIndexOf('-');
            if (lastDash <= 0) return; // invalid format

            string name = id.Substring(0, lastDash).Trim();
            string freq = id.Substring(lastDash + 1).Trim();
            if (sectorItem.IsChecked)
            {
                Logger.Debug("Adding Sector", name);
                eramViewModel.ActivatedSectors.Add(name, freq);
                if (!eramViewModel.isPlaybackMode) eramViewModel.RadarViewModel.UpdateVatsimDataService();
            }
            else
            {
                eramViewModel.ActivatedSectors.Remove(name);
                if (!eramViewModel.isPlaybackMode) eramViewModel.RadarViewModel.UpdateVatsimDataService();
            }
        }
    }
}
