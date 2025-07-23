using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization.DataContracts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services;
using vFalcon.Services.Interfaces;

namespace vFalcon.ViewModels
{
    public class ActivateSectorViewModel : ViewModelBase
    {
        private IArtccService artccService = new ArtccService();

        private string _selectedSector = string.Empty;
        private Profile profile;
        public ObservableCollection<string> ArtccOptions { get; }

        public ICommand ActivateCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? Close;
        public event Action? SectorActivated;

        public string SelectedSector
        {
            get => _selectedSector;
            set
            {
                _selectedSector = value;
                OnPropertyChanged();
                ((RelayCommand)ActivateCommand).RaiseCanExecuteChanged();
            }
        }

        public ActivateSectorViewModel(Profile profile)
        {
            this.profile = profile;
            ArtccOptions = new ObservableCollection<string>(artccService.GetArtccSectors(profile.ArtccId));

            ActivateCommand = new RelayCommand(OnActivate, () => !string.IsNullOrWhiteSpace(SelectedSector));
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnActivate()
        {
            var parts = SelectedSector.ToString().Split('-');
            string name = parts[0].Trim();
            string freq = parts[1].Trim();
            profile.LastSectorName = name;
            profile.LastSectorFreq = freq;
            SectorActivated?.Invoke();
            Close?.Invoke();
        }

        private void OnCancel()
        {
            Close?.Invoke();
        }
    }
}
