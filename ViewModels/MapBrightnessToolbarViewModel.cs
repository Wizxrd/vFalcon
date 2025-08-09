using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Behaviors;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class MapBrightnessToolbarViewModel : ViewModelBase
    {
        private BrightnessToolbarViewModel brightnessToolbarViewModel;
        private EramViewModel eramViewModel;

        private ObservableCollection<MapBrightnessButtonViewModel> brightnessBcg = new();

        public ObservableCollection<MapBrightnessButtonViewModel> BrightnessBcg
        {
            get => brightnessBcg;
            set
            {
                brightnessBcg = value;
                OnPropertyChanged();
            }
        }

        public ICommand MapBrightnessBackCommand { get; set; }
        public ICommand IncreaseBrightnessCommand { get; }
        public ICommand DecreaseBrightnessCommand { get; }

        public MapBrightnessToolbarViewModel(BrightnessToolbarViewModel brightnessToolbarViewModel, EramViewModel eramViewModel)
        {
            this.brightnessToolbarViewModel = brightnessToolbarViewModel;
            this.eramViewModel = eramViewModel;
            MapBrightnessBackCommand = new RelayCommand(OnMapBrightnessBackCommand);
            IncreaseBrightnessCommand = new RelayCommand<MapBrightnessButtonViewModel>(OnIncreaseBrightness);
            DecreaseBrightnessCommand = new RelayCommand<MapBrightnessButtonViewModel>(OnDecreaseBrightness);
            InitializeBrightnessBcg();
        }

        public void RebuildBrightnessBcg()
        {
            InitializeBrightnessBcg();
            OnPropertyChanged(nameof(BrightnessBcg));
        }

        private void InitializeBrightnessBcg()
        {
            BrightnessBcg.Clear();
            JObject profileBcgs = (JObject)eramViewModel.profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"];
            foreach (JObject geoMap in eramViewModel.ArtccGeoMaps)
            {
                if ((string)geoMap["name"] != eramViewModel.ActiveGeoMap) continue;
                JArray bcgMenu = (JArray)geoMap["bcgMenu"];
                int baseStart = eramViewModel.UseAlternateMapLayout ? 20 : 0;
                int totalVisible = 20;
                int columns = 10;
                for (int i = 0; i < bcgMenu.Count && i < baseStart + totalVisible; i++)
                {
                    int filterIndex = i + 1;
                    int mappedIndex = i - baseStart;
                    string bcg = (string)bcgMenu[i];
                    BrightnessBcg.Add(new MapBrightnessButtonViewModel
                    {
                        Index = filterIndex,
                        LabelLine1 = bcg,
                        LabelLine2 = (string)profileBcgs[$"MapGroup{filterIndex}"].ToString(),
                        Row = (mappedIndex / columns) * 2,
                        Column = (mappedIndex % columns) * 2
                    });
                }
            }
        }

        private void OnMapBrightnessBackCommand()
        {
            brightnessToolbarViewModel.OnMapBrightnessCommand();
        }

        private void OnIncreaseBrightness(MapBrightnessButtonViewModel vm)
        {
            if (int.TryParse(vm.LabelLine2, out var value) && value < 100)
            {
                vm.LabelLine2 = (value + 2).ToString();
                JObject profileBcgs = (JObject)eramViewModel.profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"];
                profileBcgs[$"MapGroup{vm.Index}"] = vm.LabelLine2;
                eramViewModel.RadarViewModel.Redraw();
            }
        }

        private void OnDecreaseBrightness(MapBrightnessButtonViewModel vm)
        {
            if (int.TryParse(vm.LabelLine2, out var value) && value > 0)
            {
                vm.LabelLine2 = (value - 2).ToString();
                JObject profileBcgs = (JObject)eramViewModel.profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"];
                profileBcgs[$"MapGroup{vm.Index}"] = vm.LabelLine2;
                eramViewModel.RadarViewModel.Redraw();
            }
        }
    }
}
