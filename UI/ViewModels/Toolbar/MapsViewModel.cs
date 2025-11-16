using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Utils;
namespace vFalcon.UI.ViewModels.Toolbar
{
    public class MapsViewModel : ViewModelBase
    {
        private ObservableCollection<string> facilities = new();
        private ObservableCollection<VideoMapViewModel> geoMaps = new();
        private ObservableCollection<string> eramGeoMaps = new();
        private string selectedFacility = string.Empty;
        private string selectedGeoMap = string.Empty;
        private Visibility isEramFacility = Visibility.Collapsed;
        MainWindowViewModel mainWindowViewModel = App.GetMainWindowViewModel();

        public ObservableCollection<string> Facilities
        {
            get => facilities;
            set
            {
                facilities = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<VideoMapViewModel> GeoMaps
        {
            get => geoMaps;
            set {
                geoMaps = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<string> EramGeoMaps
        {
            get => eramGeoMaps;
            set
            {
                eramGeoMaps = value;
                OnPropertyChanged();
            }
        }

        public string SelectedFacility
        {
            get => selectedFacility;
            set
            {
                if (!App.MainWindowViewModel.DisplayState.IsReady) return;
                selectedFacility = value;
                if (App.Artcc.id != value.Substring(0, 3) & App.Profile.MapSettings.Facility != value)
                {
                    App.Profile.MapSettings.Facility = value;
                    App.MainWindowViewModel.ActiveMaps.Clear();
                    App.Profile.ActiveStarsVideoMaps.Clear();
                    App.MainWindowViewModel.ReloadStarsFeatures(value.Substring(0, 3));
                }

                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    LoadGeoMaps();
                }
            }
        }

        public string SelectedGeoMap
        {
            get => App.Profile.MapSettings.GeoMap;
            set
            {
                if (!App.MainWindowViewModel.DisplayState.IsReady) return;
                if (geoMaps.Count < 1 && App.Profile.MapSettings.GeoMap == value)
                {
                    LoadEramGeoMaps();
                }
                else
                {
                    App.Profile.MapSettings.GeoMap = value;
                    App.Profile.ActiveEramFilters = App.MainWindowViewModel.ActiveEramMaps[value].ToHashSet();
                    selectedGeoMap = value;
                    LoadEramGeoMaps();
                    mainWindowViewModel.ReloadEramFeatures(true);
                }
                OnPropertyChanged();
            }
        }

        public Visibility IsEramFacility
        {
            get => isEramFacility;
            set
            {
                isEramFacility= value;
                OnPropertyChanged();
            }
        }

        public MapsViewModel()
        {
            LoadFacilities();
            SelectedFacility = App.Profile.MapSettings.Facility;
        }

        private static int GetStarsId(VideoMapViewModel m)
        {
            var first = m.Name?.Split(' ', 2)[0];
            return int.TryParse(first, out var n) ? n : int.MaxValue;
        }

        private void LoadFacilities()
        {
            foreach (string facility in Artcc.GetFacilities())
            {
                Facilities.Add(facility);
            }
        }

        private void PopulateEramGeoMapNames()
        {
            EramGeoMaps.Clear();
            JObject eramConfig = Artcc.GetEramConfiguration();
            JArray geoMaps = (JArray)eramConfig["geoMaps"];
            foreach (JObject geoMap in geoMaps)
            {
                EramGeoMaps.Add((string)geoMap["name"]);
            }
        }

        private void PopulateStarsGeoMaps(string selectedFacilityId)
        {
            if (mainWindowViewModel == null) return;
            GeoMaps.Clear();
            foreach (vFalcon.Models.VideoMap videoMap in vFalcon.Models.VideoMap.GetChildFacilityVideoMaps(selectedFacilityId))
            {
                int starsId = videoMap.starsId;
                VideoMapViewModel videoMapViewModel = new VideoMapViewModel
                {
                    Name = $"{starsId} {videoMap.shortName} {videoMap.name}",
                    IsChecked = mainWindowViewModel.ActiveMaps.Contains(videoMap.id)
                };
                videoMapViewModel.Command = new RelayCommand(_ =>
                {
                    if (!App.MainWindowViewModel.DisplayState.IsReady) return;
                    if (videoMapViewModel.IsChecked)
                    {
                        App.Profile.ActiveStarsVideoMaps.Add(videoMap.id);
                        mainWindowViewModel.ActiveMaps.Add(videoMap.id);
                        mainWindowViewModel.SetRenderableFeatures();
                        mainWindowViewModel.GraphicsEngine.RequestRender();
                    }
                    else
                    {
                        App.Profile.ActiveStarsVideoMaps.Remove(videoMap.id);
                        mainWindowViewModel.ActiveMaps.Remove(videoMap.id);
                        mainWindowViewModel.SetRenderableFeatures();
                        mainWindowViewModel.GraphicsEngine.RequestRender();
                    }
                });
                int insertAt = 0;
                while (insertAt < GeoMaps.Count && GetStarsId(GeoMaps[insertAt]) <= starsId)
                    insertAt++;
                GeoMaps.Insert(insertAt, videoMapViewModel);
            }
        }

        private void LoadEramGeoMaps()
        {
            if (mainWindowViewModel == null) return;
            GeoMaps.Clear();
            Dictionary<string, string> filters = vFalcon.Models.VideoMap.GetEramFilters(App.Profile.MapSettings.GeoMap);
            for (int i = 0; i < filters.Count; i++)
            {
                var kvp = filters.ElementAt(i);
                string id = kvp.Key;
                string name = kvp.Value;
                VideoMapViewModel videoMapViewModel = new VideoMapViewModel
                {
                    Name = name,
                    IsChecked = mainWindowViewModel.ActiveEramMaps[App.Profile.MapSettings.GeoMap].Contains(id)
                };
                videoMapViewModel.Command = new RelayCommand(_ =>
                {
                    if (!App.MainWindowViewModel.DisplayState.IsReady) return;
                    if (videoMapViewModel.IsChecked)
                    {
                        App.Profile.ActiveEramFilters.Add(id);
                        mainWindowViewModel.ActiveEramMaps[App.Profile.MapSettings.GeoMap].Add(id);
                        mainWindowViewModel.SetRenderableFeatures();
                        mainWindowViewModel.GraphicsEngine.RequestRender();
                    }
                    else
                    {
                        App.Profile.ActiveEramFilters.Remove(id);
                        mainWindowViewModel.ActiveEramMaps[App.Profile.MapSettings.GeoMap].Remove(id);
                        mainWindowViewModel.SetRenderableFeatures();
                        mainWindowViewModel.GraphicsEngine.RequestRender();
                    }
                });
                GeoMaps.Add(videoMapViewModel);
            }
        }

        private void LoadGeoMaps()
        {
            string selectedFacilityId = Artcc.GetFacilityIdFromName(SelectedFacility);
            GeoMaps.Clear();
            if (App.Artcc.id == selectedFacilityId)
            {
                IsEramFacility = Visibility.Visible;
                if (EramGeoMaps.Count < 1) PopulateEramGeoMapNames();
                SelectedGeoMap = App.Profile.MapSettings.GeoMap;
            }
            else
            {
                IsEramFacility = Visibility.Collapsed;
                PopulateStarsGeoMaps(selectedFacilityId);
            }
        }
    }
}
