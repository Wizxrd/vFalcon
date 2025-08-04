using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vFalcon.ViewModels
{
    public class GeneralSettingsViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;

        private string selectedGeoMapSet;
        public string SelectedGeoMapSet
        {
            get => selectedGeoMapSet;
            set
            {
                if (selectedGeoMapSet != value)
                {
                    selectedGeoMapSet = value;
                    eramViewModel.ActiveGeoMap = value;
                    eramViewModel.SwapGeoMapSet();
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> GeoMapSets { get; }

        private bool isTopDownMode;
        public bool IsTopDownMode
        {
            get => isTopDownMode;
            set
            {
                if (isTopDownMode != value)
                {
                    isTopDownMode = value;
                    eramViewModel.TdmActive = value;
                    OnPropertyChanged(nameof(IsTopDownMode));
                }
            }
        }



        public GeneralSettingsViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            IsTopDownMode = eramViewModel.TdmActive;
            GeoMapSets = new ObservableCollection<string>(eramViewModel.ArtccGeoMaps.Select(g => g["name"]?.ToString()).Where(n => !string.IsNullOrEmpty(n)));
            selectedGeoMapSet = eramViewModel.ActiveGeoMap;
        }
    }
}
