using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vFalcon.Helpers;

namespace vFalcon.ViewModels
{
    public class MasterToolbarViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;

        public string ActiveGeoMap
        {
            get => eramViewModel.ActiveGeoMap;
            set
            {
                eramViewModel.ActiveGeoMap = value;
                OnPropertyChanged();
            }
        }

        public MasterToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            ActiveGeoMap = eramViewModel.ActiveGeoMap;
        }
    }
}
