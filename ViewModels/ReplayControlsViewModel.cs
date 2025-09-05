using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace vFalcon.ViewModels
{
    class ReplayControlsViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;
        private int maximumSliderValue = 0;
        private int sliderValueTick = 0;
        private string elapsedTimeTick = string.Empty;

        public int MaximumSliderValue
        {
            get => maximumSliderValue;
            set
            {
                maximumSliderValue = value;
                OnPropertyChanged();
            }
        }

        public int SliderValueTick
        {
            get => sliderValueTick;
            set
            {
                sliderValueTick = value;
                eramViewModel.playbackService.Tick = value;
                OnPropertyChanged();
            }
        }

        public string ElapsedTimeTick
        {
            get => elapsedTimeTick;
            set
            {
                elapsedTimeTick = value;
                OnPropertyChanged();
            }
        }

        public ReplayControlsViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
        }
    }
}
