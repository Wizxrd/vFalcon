using Microsoft.VisualBasic.Logging;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Views;

namespace vFalcon.ViewModels
{
    public class MasterToolbarViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;

        private object? mapBrightnessContent;

        public bool MapBrightnessOpen = false;

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand BrightnessCommand { get; }
        public ICommand MapsCommand { get; }
        public ICommand MapBrightnessCommand { get; set; }

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public string MapsLabelLine1 => eramViewModel.MapsLabelLine1;
        public string MapsLabelLine2 => eramViewModel.MapsLabelLine2;
        public int VelocityVector => eramViewModel.VelocityVector;

        public string ZoomLevel
        {
            get => eramViewModel.ZoomLevel;
            set { eramViewModel.ZoomLevel = value; OnPropertyChanged(); }
        }

        public object? MapBrightnessContent
        {
            get => mapBrightnessContent;
            set
            {
                mapBrightnessContent = value;
                OnPropertyChanged();
            }
        }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public MasterToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            MapsCommand = new RelayCommand(() => eramViewModel.OnMapsCommand());
            ZoomLevel = eramViewModel.ZoomLevel;
            eramViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(eramViewModel.ZoomLevel))
                    OnPropertyChanged(nameof(ZoomLevel));
                if (e.PropertyName == nameof(eramViewModel.MapsLabelLine1))
                    OnPropertyChanged(nameof(MapsLabelLine1));
                if (e.PropertyName == nameof(eramViewModel.MapsLabelLine2))
                    OnPropertyChanged(nameof(MapsLabelLine2));
                if (e.PropertyName == nameof(eramViewModel.VelocityVector))
                    OnPropertyChanged(nameof(VelocityVector));
            };
        }
    }
}
