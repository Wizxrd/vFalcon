using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using vFalcon.Commands;

namespace vFalcon.ViewModels
{
    public class ToolbarControlViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;

        public ICommand MasterToolbarCommand { get; }

        public bool IsMasterToolbarOpen
        {
            get => eramViewModel.IsMasterToolbarOpen;
            set
            {
                eramViewModel.IsMasterToolbarOpen = value;
                OnPropertyChanged();
            }
        }

        private Visibility toolbarControlGrid = Visibility.Collapsed;
        public Visibility ToolbarControlGrid
        {
            get => toolbarControlGrid;
            set
            {
                if (toolbarControlGrid == Visibility.Visible)
                {
                    toolbarControlGrid = Visibility.Collapsed;
                }
                else
                {
                    toolbarControlGrid = Visibility.Visible;
                }
                OnPropertyChanged();
            }
        }

        public ICommand ToolbarControlCommand { get; }

        public ToolbarControlViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            IsMasterToolbarOpen = eramViewModel.IsMasterToolbarOpen;
            MasterToolbarCommand = new RelayCommand(() => eramViewModel.OnMasterToolbar());
            ToolbarControlCommand = new RelayCommand(OnToolbarControl);
        }

        private void OnToolbarControl()
        {
            if (ToolbarControlGrid == Visibility.Visible)
            {
                ToolbarControlGrid = Visibility.Collapsed;
                return;
            }
            ToolbarControlGrid = Visibility.Visible;
        }
    }
}
