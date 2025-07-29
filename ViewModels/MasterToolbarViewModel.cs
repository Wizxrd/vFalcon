using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;

namespace vFalcon.ViewModels
{
    public class MasterToolbarViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;

        public ICommand BrightnessCommand { get; }
        public ICommand CursorCommand { get; }
        public ICommand MapsCommand { get; }

        public string LabelLine1
        {
            get => eramViewModel.MapsLabelLine1;
            set
            {
                eramViewModel.MapsLabelLine1 = value;
                OnPropertyChanged();
            }
        }

        public string LabelLine2
        {
            get => eramViewModel.MapsLabelLine2;
            set
            {
                eramViewModel.MapsLabelLine2 = value;
                OnPropertyChanged();
            }
        }

        public MasterToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            BrightnessCommand = new RelayCommand(() => eramViewModel.OnBrightnessCommand());
            CursorCommand = new RelayCommand(() => eramViewModel.OnCursorCommand());
            MapsCommand = new RelayCommand(() => eramViewModel.OnMapsCommand());
            LabelLine1 = eramViewModel.MapsLabelLine1;
            LabelLine2 = eramViewModel.MapsLabelLine2;
        }
    }
}
