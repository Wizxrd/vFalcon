using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Views;

namespace vFalcon.ViewModels
{
    public class BrightnessToolbarViewModel : ViewModelBase
    {
        // ========================================================
        //                     FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;
        private MapBrightnessToolbarView mapBrightnessView;

        private object? mapBrightnessContent;

        public bool MapBrightnessOpen = false;

        // ========================================================
        //                     PROPERTIES
        // ========================================================

        public object? MapBrightnessContent
        {
            get => mapBrightnessContent;
            set
            {
                mapBrightnessContent = value;
                OnPropertyChanged();
            }
        }

        public int BackgroundValue
        {
            get => eramViewModel.BackgroundValue;
            set
            {
                eramViewModel.BackgroundValue = value;
                eramViewModel.UpdateBackground();
                OnPropertyChanged();
            }
        }

        public int BrightnessValue
        {
            get => eramViewModel.BrightnessValue;
            set
            {
                eramViewModel.BrightnessValue = value;
                eramViewModel.UpdateBackground();
                OnPropertyChanged();
            }
        }

        // ========================================================
        //                     COMMANDS
        // ========================================================
        public ICommand BrightnessBackCommand { get; set; }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public BrightnessToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;


            BrightnessBackCommand = new RelayCommand(() => eramViewModel.OnBrightnessCommand());
        }

        public void OpenMapBrightness()
        {
            MapBrightnessContent = mapBrightnessView;
            MapBrightnessOpen = true;
        }

        public void CloseMapBrightness()
        {
            MapBrightnessContent = null;
            MapBrightnessOpen = false;
        }

        public void OnMapBrightnessCommand()
        {
            if (!MapBrightnessOpen)
            {
                OpenMapBrightness();
            }
            else
            {
                CloseMapBrightness();
            }
        }
    }
}
