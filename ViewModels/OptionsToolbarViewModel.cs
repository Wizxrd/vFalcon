using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Service;
using vFalcon.Views;
using MessageBox = vFalcon.Services.Service.MessageBoxService;

namespace vFalcon.ViewModels
{
    public class OptionsToolbarViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;
        private RadarViewModel radarViewModel;

        private string recordingText = "Start Recording";
        private string recordingInput = "Alt+R";
        private string showLatLonText = "Show Lat/Lon";

        private bool exitReplayIsEnabled = false;

        public string ShowLatLonText
        {
            get => showLatLonText;
            set
            {
                showLatLonText = value;
                OnPropertyChanged();
            }
        }

        public bool ExitReplayIsEnabled
        {
            get => exitReplayIsEnabled;
            set
            {
                exitReplayIsEnabled = value;
                OnPropertyChanged();
            }
        }

        public string RecordingText
        {
            get => recordingText;
            set
            {
                recordingText = value;
                OnPropertyChanged();
            }
        }

        public string RecordingInput
        {
            get => recordingInput;
            set
            {
                recordingInput = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenPositionsCommand { get; set; }
        public ICommand StartRecordingCommand { get; set; }
        public ICommand LoadReplayCommand { get; set; }
        public ICommand ExitReplayCommand { get; set; }
        public ICommand SaveProfileCommand { get; set; }
        public ICommand SwitchProfileCommand { get; set; }
        public ICommand OpenDiscordCommand { get; set; }
        public ICommand OpenGithubCommand { get; set; }
        public ICommand OpenMapsCommand { get; set; }
        public ICommand OpenSettingsCommand {  get; set; }

        public ICommand OpenSearchCommand { get; set; }
        public ICommand OpenFindCommand { get; set; }

        public ICommand ShowLatLonCommand { get; set; }

        public ICommand OpenAircraftListCommand { get; set; }
        public ICommand OpenControllersListCommand { get; set; }
        public ICommand CaptureScreenCommand { get; set; }

        public ICommand SaveProfileAsCommand { get; set; }

        public OptionsToolbarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            radarViewModel = eramViewModel.RadarViewModel;
            OpenPositionsCommand = new RelayCommand(OnOpenPositionsCommand);
            StartRecordingCommand = new RelayCommand(OnStartRecordingCommand);
            LoadReplayCommand = new RelayCommand(OnLoadReplayCommand);
            ExitReplayCommand = new RelayCommand(OnExitReplayCommand);
            SaveProfileCommand = new RelayCommand(OnSaveProfileCommand);
            SaveProfileAsCommand = new RelayCommand(OnSaveProfileAsCommand);
            SwitchProfileCommand = new RelayCommand(OnSwitchProfileCommand);
            OpenDiscordCommand = new RelayCommand(OnOpenDiscordCommand);
            OpenGithubCommand = new RelayCommand(OnOpenGithubCommand);
            OpenMapsCommand = new RelayCommand(OnOpenMapsCommand);
            OpenSettingsCommand = new RelayCommand(OnOpenSettingsCommand);
            OpenSearchCommand = new RelayCommand(OnOpenSearchCommand);
            OpenFindCommand = new RelayCommand(OnOpenFindCommand);
            ShowLatLonCommand = new RelayCommand(OnShowLatLonCommand);
            OpenAircraftListCommand = new RelayCommand(OnOpenAircraftListCommand);
            OpenControllersListCommand = new RelayCommand(OnOpenControllersListCommand);
            CaptureScreenCommand = new RelayCommand(OnCaptureScreenCommand);
        }
        ProfileService profileService = new ProfileService();

        private void OnSaveProfileAsCommand()
        {
            SaveProfileAsView saveProfileAsView = new(eramViewModel);
            saveProfileAsView.Owner = eramViewModel.eramView;
            saveProfileAsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            saveProfileAsView.ShowDialog();
        }

        private void OnSaveProfileCommand()
        {
            var confirmed = MessageBox.Confirm($"Save profile: \"{eramViewModel.profile.Name}\"?");
            if (!confirmed) return;
            profileService.Save(eramViewModel.profile);
        }

        private void OnCaptureScreenCommand()
        {
            eramViewModel.OnCaptureScreenCommand();
        }

        public AircraftListToolbarView? aircraftListToolbarView;
        private void OnOpenAircraftListCommand()
        {
            if (aircraftListToolbarView != null) return;
            aircraftListToolbarView = new AircraftListToolbarView(eramViewModel);
            aircraftListToolbarView.Owner = eramViewModel.eramView;
            aircraftListToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            aircraftListToolbarView.Closed += (_, __) => aircraftListToolbarView = null;
            aircraftListToolbarView.Show();
        }

        public ControllersListView? controllersListView;
        private void OnOpenControllersListCommand()
        {
            if (controllersListView != null) return;
            controllersListView = new ControllersListView(eramViewModel);
            controllersListView.Owner = eramViewModel.eramView;
            controllersListView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            controllersListView.Closed += (_, __) =>
            {
                controllersListView.controllersListViewModel.StopRefreshTimer();
                controllersListView = null;
            };
            controllersListView.Show();
        }

        private void OnShowLatLonCommand()
        {
            if (!eramViewModel.ShowLatLon)
            {
                eramViewModel.ShowLatLon = true;
                ShowLatLonText = "Hide Lat/Lon";
            }
            else
            {
                eramViewModel.ShowLatLon = false;
                ShowLatLonText = "Show Lat/Lon";
                eramViewModel.MouseCoordinates = string.Empty;
            }
        }

        private void OnOpenFindCommand()
        {
            FindOptionsToolbarView findOptionsToolbarView = new FindOptionsToolbarView(eramViewModel);
            findOptionsToolbarView.Owner = eramViewModel.eramView;
            findOptionsToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            findOptionsToolbarView.ShowDialog();
        }

        private void OnOpenSearchCommand()
        {
            SearchOptionsToolbarView searchOptionsToolbarView = new SearchOptionsToolbarView(eramViewModel);
            searchOptionsToolbarView.Owner = eramViewModel.eramView;
            searchOptionsToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            searchOptionsToolbarView.ShowDialog();
        }

        private void OnOpenPositionsCommand()
        {
            PositionsToolbarView positionsToolbarView = new PositionsToolbarView(eramViewModel);
            positionsToolbarView.Owner = eramViewModel.eramView;
            positionsToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            positionsToolbarView.ShowDialog();
        }

        private void OnOpenSettingsCommand()
        {
            eramViewModel.eramView.OpenGeneralSettingsWindow();
        }

        private MapsOptionsToolbarView mapsOptionsToolbarView;
        private void OnOpenMapsCommand()
        {
            if (mapsOptionsToolbarView != null) return;
            mapsOptionsToolbarView = new MapsOptionsToolbarView(eramViewModel);
            mapsOptionsToolbarView.Owner = eramViewModel.eramView;
            mapsOptionsToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            mapsOptionsToolbarView.Closed += (_, __) => mapsOptionsToolbarView = null;
            mapsOptionsToolbarView.Show();
        }

        private void OnOpenGithubCommand()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Wizxrd/vFalcon",
                UseShellExecute = true
            });
        }

        private void OnOpenDiscordCommand()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/8g7MFs5QfB",
                UseShellExecute = true
            });
        }

        private void OnSwitchProfileCommand()
        {
            eramViewModel.eramView.OpenLoadProfileWindow();
        }

        private void OnExitReplayCommand()
        {
            if (ExitReplayIsEnabled)
            {
                eramViewModel.ExitRecording();
                ExitReplayIsEnabled = false;
                return;
            }
        }

        private void OnLoadReplayCommand()
        {
            if (!eramViewModel.IsRecording)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = Loader.LoadFolder("Recordings"),
                    Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                    Title = "Select a JSON File"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    eramViewModel.OnLoadRecording(selectedFilePath);
                    ExitReplayIsEnabled = true;
                }
            }
            else
            {
                var confirmed = MessageBox.Confirm("Cannot load a replay while recording");
            }
        }
        private void OnStartRecordingCommand()
        {
            eramViewModel.OnToggleRecording();
            if (eramViewModel.IsRecording)
            {
                RecordingText = "Stop Recording";
                RecordingInput = "Alt+R";
            }
            else
            {
                RecordingText = "Start Recording";
                RecordingInput = "Alt+R";
            }
        }
    }
}
