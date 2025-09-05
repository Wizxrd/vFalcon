using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Models;
using vFalcon.Services.Service;
using vFalcon.Helpers;

namespace vFalcon.ViewModels
{
    public class ActivateSectorViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly Artcc artcc;
        private readonly Profile profile;
        private string selectedSector = string.Empty;
        private bool activateButtonEnabled { get; set; } = false;

        // ========================================================
        //                      PROPERTIES
        // ========================================================
        public ObservableCollection<string> ArtccSectors { get; }

        public bool ActivateButtonEnabled
        {
            get => activateButtonEnabled;
            set
            {
                activateButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public string SelectedSector
        {
            get => selectedSector;
            set
            {
                selectedSector = value;
                OnPropertyChanged();
                ActivateButtonEnabled = true;
                ((RelayCommand)ActivateCommand).RaiseCanExecuteChanged();
            }
        }

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand ActivateCommand { get; set; }
        public ICommand CancelCommand { get; }

        // ========================================================
        //                      EVENTS
        // ========================================================
        public event Action? Close;
        public event Action? SectorActivated;

        // ========================================================
        //                      CONSTRUCTOR
        // ========================================================
        public ActivateSectorViewModel(Artcc artcc, Profile profile)
        {
            this.artcc = artcc;
            this.profile = profile;

            ArtccSectors = new ObservableCollection<string>(ArtccService.GetArtccSectors(profile.ArtccId));

            ActivateCommand = new RelayCommand(OnActivate);
            CancelCommand = new RelayCommand(OnCancel);

            AutoSelectLastPosition();
        }

        // ========================================================
        //                      METHODS
        // ========================================================

        private void AutoSelectLastPosition()
        {

            foreach (JObject position in (JArray)artcc.facility["positions"])
            {
                if (profile.LastUsedPositionId == (string)position["id"])
                {
                    string name = (string)position["name"];
                    long frequencyHz = position["frequency"]?.ToObject<long>() ?? 0;
                    double frequencyMHz = frequencyHz / 1_000_000.0;
                    string display = $"{name} - {frequencyMHz:F3}";
                    SelectedSector = display;   // ✅ Matches the format in ArtccSectors
                    ActivateButtonEnabled = true;
                    break;
                }
            }
        }
        private void OnActivate()
        {
            int lastDash = SelectedSector.LastIndexOf('-');
            if (lastDash <= 0) return; // invalid format

            string name = SelectedSector.Substring(0, lastDash).Trim();
            string freq = SelectedSector.Substring(lastDash + 1).Trim();

            Logger.Debug("TEST", $"{name} | {freq}");
            profile.ActivatedSectorName = name;
            profile.ActivatedSectorFreq = freq;
            SectorActivated?.Invoke();
            Close?.Invoke();
        }

        private void OnCancel()
        {
            Close?.Invoke();
        }
    }
}
