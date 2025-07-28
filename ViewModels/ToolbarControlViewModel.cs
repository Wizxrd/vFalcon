using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        // Fields
        private EramViewModel eramViewModel;
        private Visibility toolbarControlGrid = Visibility.Collapsed;

        // Commands
        public ICommand MasterToolbarCommand { get; }
        public ICommand ToolbarControlCommand { get; }
        public ICommand SwapZOrderCommand { get; }

        // Properties
        public bool IsMasterToolbarOpen
        {
            get => eramViewModel.IsMasterToolbarOpen;
            set
            {
                eramViewModel.IsMasterToolbarOpen = value;
                OnPropertyChanged();
            }
        }

        public string MasterRaiseLower => eramViewModel.MasterRaiseLower;

        public bool IsRaiseMasterToolbar
        {
            get => eramViewModel.IsRaiseMasterToolbar;
            set
            {
                eramViewModel.IsRaiseMasterToolbar = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MasterRaiseLower));
            }
        }

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

        // Constructor
        public ToolbarControlViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;

            eramViewModel.PropertyChanged += OnPropertyChanged;

            IsMasterToolbarOpen = eramViewModel.IsMasterToolbarOpen;
            IsRaiseMasterToolbar = eramViewModel.IsRaiseMasterToolbar;

            MasterToolbarCommand = new RelayCommand(() => eramViewModel.OnMasterToolbar());
            SwapZOrderCommand = new RelayCommand(() => eramViewModel.SwapZOrder());
            ToolbarControlCommand = new RelayCommand(OnToolbarControl);
        }

        // Methods
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EramViewModel.MasterRaiseLower))
                OnPropertyChanged(nameof(MasterRaiseLower));

            if (e.PropertyName == nameof(EramViewModel.IsRaiseMasterToolbar))
                OnPropertyChanged(nameof(IsRaiseMasterToolbar));
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
