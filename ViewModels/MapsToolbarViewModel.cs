using Newtonsoft.Json.Linq;
using OpenTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class MapsToolbarViewModel : ViewModelBase
    {

        private EramViewModel eramViewModel;

        private ObservableCollection<MapFilter> mapFilters = new ObservableCollection<MapFilter>();

        public ICommand MapsBackCommand { get; }

        public string MapsLabelLine1
        {
            get => eramViewModel.MapsLabelLine1;
            set
            {
                eramViewModel.MapsLabelLine1 = value;
                OnPropertyChanged();
            }
        }

        public string MapsLabelLine2
        {
            get => eramViewModel.MapsLabelLine2;
            set
            {
                eramViewModel.MapsLabelLine2 = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<MapFilter> MapFilters
        {
            get => mapFilters;
            set
            {
                mapFilters = value;
                OnPropertyChanged();
            }
        }

        public MapsToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            MapsBackCommand = new RelayCommand(() => eramViewModel.OnMapsCommand());
            MapsLabelLine1 = eramViewModel.MapsLabelLine1;
            MapsLabelLine2 = eramViewModel.MapsLabelLine2;
            InitializeGeoMapSet();
        }

        private void InitializeGeoMapSet()
        {
            MapFilters.Clear();
            foreach (JObject geoMap in eramViewModel.ArtccGeoMaps)
            {
                if ((string)geoMap["name"] == eramViewModel.ActiveGeoMap)
                {
                    JArray filterMenu = (JArray)geoMap["filterMenu"];
                    int index = 0;

                    foreach (JObject filter in filterMenu)
                    {
                        string id = (string)filter["id"];
                        MapFilters.Add(new MapFilter
                        {
                            Id = id,
                            LabelLine1 = (string)filter["labelLine1"],
                            LabelLine2 = (string)filter["labelLine2"],
                            Row = index < 20 ? 0 : 2,
                            Column = (index % 20) * 2,
                            Command = new RelayCommand(_ => ToggleMapCommand(id))
                        });
                        index++;
                    }
                    break;
                }
            }
        }

        private void ToggleMapCommand(string id)
        {
            Logger.Debug("MapsToolbarViewModel.ToggleMapCommand", id);
        }
    }
}
