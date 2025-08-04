using System.Windows.Input;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class BrightnessToolbarViewModel : ViewModelBase
    {
        // ========================================================
        //                     FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;

        // ========================================================
        //                     PROPERTIES
        // ========================================================
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
        public ICommand BrightnessBackCommand { get; }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public BrightnessToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            BrightnessBackCommand = new RelayCommand(() => eramViewModel.OnBrightnessCommand());
        }
    }
}
