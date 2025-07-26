using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ToolbarControlViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            IsMasterToolbarOpen = eramViewModel.IsMasterToolbarOpen;
            MasterToolbarCommand = new RelayCommand(() => eramViewModel.OnMasterToolbar());
        }
    }
}
