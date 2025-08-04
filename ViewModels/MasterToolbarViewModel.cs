using System.Windows.Input;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class MasterToolbarViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand BrightnessCommand { get; }
        public ICommand CursorCommand { get; }
        public ICommand MapsCommand { get; }

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public string MapsLabelLine1 => eramViewModel.MapsLabelLine1;
        public string MapsLabelLine2 => eramViewModel.MapsLabelLine2;

        public string ZoomLevel
        {
            get => eramViewModel.ZoomLevel;
            set { eramViewModel.ZoomLevel = value; OnPropertyChanged(); }
        }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public MasterToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;

            BrightnessCommand = new RelayCommand(() => eramViewModel.OnBrightnessCommand());
            CursorCommand = new RelayCommand(() => eramViewModel.OnCursorCommand());
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
            };
        }
    }
}
