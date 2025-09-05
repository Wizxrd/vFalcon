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
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;
        private string selectedGeoMapSet;
        private bool isTopDownMode;

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public string SelectedGeoMapSet
        {
            get => selectedGeoMapSet;
            set
            {
                if (selectedGeoMapSet != value)
                {
                    selectedGeoMapSet = value;
                    eramViewModel.ActiveGeoMap = value;
                    _ = eramViewModel.SwapGeoMapSet();
                    Close?.Invoke();
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> GeoMapSets { get; }

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

        // ========================================================
        //                      EVENTS
        // ========================================================
        public event Action? Close;
        // ========================================================
        //                      CONSTRUCTOR
        // ========================================================
        public GeneralSettingsViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            IsTopDownMode = eramViewModel.TdmActive;

            GeoMapSets = new ObservableCollection<string>(
                eramViewModel.ArtccGeoMaps.Select(g => g["name"]?.ToString()).Where(n => !string.IsNullOrEmpty(n))
            );

            selectedGeoMapSet = eramViewModel.ActiveGeoMap;
        }
    }
}
