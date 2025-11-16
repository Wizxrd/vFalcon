using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class MapOptions : ViewModelBase
    {
        private string _name = string.Empty;
        private bool _isChecked;
        private string starsId = string.Empty;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged();
                Command?.Execute(this);
            }
        }

        public string StarsId
        {
            get => starsId;
            set
            {
                starsId = value;
                OnPropertyChanged();
            }
        }

        public ICommand Command { get; set; }
    }

    public class MapsOptionsToolbarViewModel : ViewModelBase
    {
        private readonly EramViewModel eramViewModel;

        public ObservableCollection<MapOptions> mapOptions = new();
        public ObservableCollection<MapOptions> MapOptions
        {
            get => mapOptions;
            set { mapOptions = value; OnPropertyChanged(); }
        }
        public MapsOptionsToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            InitializeGeoMapSet();
        }

        public void RebuildFilters()
        {
            InitializeGeoMapSet();
            OnPropertyChanged(nameof(MapOptions));
            Logger.Debug("Rebuild Filters", "Clearing and resetting");
        }

        int GetStarsId(MapOptions m)
        {
            var first = m.Name?.Split(' ', 2)[0];
            return int.TryParse(first, out var n) ? n : int.MaxValue;
        }

        private void InitializeGeoMapSet()
        {
            bool _buildingMapOptions = true;

            MapOptions.Clear();
            if (eramViewModel.profile.DisplayType == "STARS")
            {
                int filterIndex = 0;
                foreach (string id in eramViewModel.ActiveVideoMapIds)
                {
                    foreach (JObject videoMap in eramViewModel.artcc.videoMaps)
                    {
                        if (id == (string)videoMap["id"])
                        {
                            int localIndex = filterIndex; // capture safely per item
                            bool isActive = eramViewModel.StarsActiveFilters.Contains(id);

                            int starsId = videoMap["starsId"]?.Value<int>() ?? 0;

                            var item = new MapOptions
                            {
                                Name = $"{starsId} {videoMap["shortName"]} {videoMap["name"]}",
                                IsChecked = isActive
                            };

                            item.Command = new RelayCommand(_ =>
                            {
                                if (_buildingMapOptions) return; // ignore programmatic changes

                                if (item.IsChecked)
                                {
                                    if (!eramViewModel.StarsActiveFilters.Contains(id))
                                    {
                                        eramViewModel.StarsActiveFilters.Add(id);
                                        eramViewModel.profile.MapFilters[$"{id}"] = localIndex;
                                    }
                                }
                                else
                                {
                                    if (eramViewModel.StarsActiveFilters.Contains(id))
                                    {
                                        eramViewModel.StarsActiveFilters.Remove(id);
                                        eramViewModel.profile.MapFilters.Remove(id.ToString());
                                    }
                                }

                                eramViewModel.RadarViewModel.Redraw();
                            });
                            int insertAt = 0;
                            while (insertAt < MapOptions.Count && GetStarsId(MapOptions[insertAt]) <= starsId)
                                insertAt++;
                            MapOptions.Insert(insertAt, item);
                            filterIndex++;

                            break; // stop scanning once we matched this id
                        }
                    }
                }
            }
            else if (eramViewModel.profile.DisplayType == "ERAM")
            {
                foreach (JObject geoMap in eramViewModel.ArtccGeoMaps)
                {
                    if ((string)geoMap["name"] != eramViewModel.ActiveGeoMap)
                        continue;

                    var filterMenu = (JArray)geoMap["filterMenu"];

                    for (int i = 0; i < 40; i++)
                    {
                        var filter = (JObject)filterMenu[i];
                        string name = $"{(string)filter["labelLine1"]} {(string)filter["labelLine2"]}";
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        int filterIndex = i + 1;
                        bool isActive = eramViewModel.ActiveFilters.Contains(filterIndex);
                        var item = new MapOptions
                        {
                            Name = name,
                            IsChecked = isActive
                        };

                        item.Command = new RelayCommand(_ =>
                        {
                            if (item.IsChecked)
                            {
                                if (!eramViewModel.ActiveFilters.Contains(filterIndex))
                                {
                                    eramViewModel.ActiveFilters.Add(filterIndex);
                                    eramViewModel.profile.MapFilters[$"{filterIndex}"] = filterIndex;
                                }
                            }
                            else
                            {
                                if (eramViewModel.ActiveFilters.Contains(filterIndex))
                                    eramViewModel.ActiveFilters.Remove(filterIndex);
                                eramViewModel.profile.MapFilters.Remove(filterIndex.ToString());
                                //eramViewModel.profile.MapFilters[$"{filterIndex}"] = null;
                            }
                            eramViewModel.RadarViewModel.Redraw();
                        });
                        MapOptions.Add(item);
                    }

                    break;
                }
            }
            _buildingMapOptions = false;
        }
    }
}
