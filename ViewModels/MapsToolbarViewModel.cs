using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Models;
using vFalcon.Helpers;

namespace vFalcon.ViewModels
{
    public class MapsToolbarViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;
        private ObservableCollection<MapFilter> mapFilters = new();

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand MapsBackCommand { get; }

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public string MapsLabelLine1 => eramViewModel.MapsLabelLine1;
        public string MapsLabelLine2 => eramViewModel.MapsLabelLine2;

        public ObservableCollection<MapFilter> MapFilters
        {
            get => mapFilters;
            set { mapFilters = value; OnPropertyChanged(); }
        }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public MapsToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;

            MapsBackCommand = new RelayCommand(() => eramViewModel.OnMapsCommand());

            eramViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(eramViewModel.MapsLabelLine1))
                    OnPropertyChanged(nameof(MapsLabelLine1));
                if (e.PropertyName == nameof(eramViewModel.MapsLabelLine2))
                    OnPropertyChanged(nameof(MapsLabelLine2));
            };

            InitializeGeoMapSet();
        }

        // ========================================================
        //                  PRIVATE METHODS
        // ========================================================

        public void RebuildFilters()
        {
            InitializeGeoMapSet();
            OnPropertyChanged(nameof(MapFilters));
        }

        private void InitializeGeoMapSet()
        {
            MapFilters.Clear();
            foreach (JObject geoMap in eramViewModel.ArtccGeoMaps)
            {
                if ((string)geoMap["name"] != eramViewModel.ActiveGeoMap)
                    continue;

                JArray filterMenu = (JArray)geoMap["filterMenu"];

                int baseStart = eramViewModel.UseAlternateMapLayout ? 20 : 0;

                int totalVisible = 20;
                int columns = 10;

                for (int i = baseStart; i < filterMenu.Count && i < baseStart + totalVisible; i++)
                {
                    JObject filter = (JObject)filterMenu[i];
                    int filterIndex = i + 1;
                    bool isActive = eramViewModel.ActiveFilters.Contains(filterIndex);

                    int mappedIndex = i - baseStart;

                    MapFilters.Add(new MapFilter
                    {
                        Id = (string)filter["id"],
                        Index = filterIndex,
                        LabelLine1 = (string)filter["labelLine1"],
                        LabelLine2 = (string)filter["labelLine2"],
                        Row = (mappedIndex / columns) * 2,
                        Column = (mappedIndex % columns) * 2,
                        Command = new RelayCommand(_ => ToggleMapCommand(filterIndex)),
                        IsActive = isActive,
                        IsChecked = isActive
                    });
                }
                break;
            }
        }

        private void ToggleMapCommand(int filterIndex)
        {
            if (eramViewModel.ActiveFilters.Contains(filterIndex))
            {
                eramViewModel.ActiveFilters.Remove(filterIndex);
            }
            else
            {
                eramViewModel.ActiveFilters.Add(filterIndex);
            }
            eramViewModel.RadarViewModel.Redraw();
        }
    }
}
