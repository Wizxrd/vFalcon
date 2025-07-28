using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class BrightnessToolbarViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;

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

        public ICommand BrightnessBackCommand { get; }
        public BrightnessToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            BrightnessBackCommand = new RelayCommand(() => eramViewModel.OnBrightnessCommand());
        }
    }
} 
