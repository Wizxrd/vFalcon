using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services;
using vFalcon.Services.Interfaces;
using vFalcon.Views;

namespace vFalcon.ViewModels
{
    public class EramViewModel : ViewModelBase
    {
        private readonly IProfileService profileService = new ProfileService();

        private string _zuluTime;
        private DispatcherTimer zuluTimer = new DispatcherTimer();
        private Dictionary<string, Func<bool>> mapEnabledGetters;
        private Dictionary<string, Action<bool>> mapEnabledSetters;

        private string _cursorSize;
        private string _activateButtonLabel = "ACTIVATE";
        private string _profileName;
        private bool _isCursorVisible = true;
        private bool _isBndryEnabled;
        private bool _isAppchEnabled;
        private bool _isLowsEnabled;
        private bool _isHighsEnabled;

        public event Action? RequestSwitchProfile;
        public event Action? RequestSaveProfile;
        public Func<string, Task<bool>> RequestConfirmation;
        public event Action? RequestSaveProfileAs;

        public RadarViewModel RadarViewModel { get; }

        public ICommand ToggleSectorCommand { get; }

        public ICommand SwitchProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand SaveProfileAsCommand { get; }
        public ICommand DecreaseCursorSizeCommand { get; }
        public ICommand IncreaseCursorSizeCommand { get; }
        public ICommand ToggleVideoMapCommand { get; }

        public Profile profile { get; set; }

        public string ZuluTime
        {
            get => _zuluTime;
            set
            {
                if (_zuluTime != value)
                {
                    _zuluTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ActivateButtonLabel
        {
            get => _activateButtonLabel;
            set
            {
                _activateButtonLabel = value;
                OnPropertyChanged();
            }
        }

        public string CursorSize
        {
            get => profile.CursorSize;
            set
            {
                if (profile.CursorSize != value)
                {
                    profile.CursorSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCursorVisible
        {
            get => _isCursorVisible;
            set
            {
                _isCursorVisible = value;
                OnPropertyChanged();
            }
        }

        public string ProfileName
        {
            get => _profileName;
            set
            {
                if (value != _profileName)
                {
                    _profileName = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsBndryEnabled
        {
            get => profile.BndryEnabled;
            set
            {
                profile.BndryEnabled = value;
                OnPropertyChanged();
            }
        }


        public bool IsAppchEnabled
        {
            get => profile.AppchCntlEnabled;
            set
            {
                profile.AppchCntlEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsLowsEnabled
        {
            get => profile.LowsEnabled;
            set
            {
                profile.LowsEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsHighsEnabled
        {
            get => profile.HighsEnabled;
            set
            {
                profile.HighsEnabled = value;
                OnPropertyChanged();
            }
        }

        public EramViewModel(Profile profile)
        {
            StartZuluTimer();

            this.profile = profile;
            RadarViewModel = new RadarViewModel(profile);
            RadarViewModel.SetCursorVisibility = visible => IsCursorVisible = visible;

            SetProfileName();
            InitializeMapDictionaries();
            InitializeProfile();

            ToggleSectorCommand = new RelayCommand<ToggleButton>(OnToggleSector);

            SwitchProfileCommand = new RelayCommand(OnSwitchProfile);
            SaveProfileCommand = new RelayCommand(OnSaveProfile);
            SaveProfileAsCommand = new RelayCommand(OnSaveProfileAs);

            DecreaseCursorSizeCommand = new RelayCommand(DecreaseCursorSize);
            IncreaseCursorSizeCommand = new RelayCommand(IncreaseCursorSize);

            ToggleVideoMapCommand = new RelayCommand(param => ToggleVideoMap(param?.ToString()));
        }

        private void OnToggleSector(ToggleButton button)
        {
            if (button.IsChecked == true)
            {
                var vm = new ActivateSectorViewModel(profile);
                bool confirmed = false;

                vm.SectorActivated += () =>
                {
                    SetProfileName();
                    RadarViewModel.UpdateVatsimDataService();
                    confirmed = true;
                };

                var view = new ActivateSectorView(vm)
                {
                    Owner = Application.Current.MainWindow
                };
                vm.Close += () => view.Close();
                view.ShowDialog();

                if (confirmed)
                {
                    ActivateButtonLabel = "DEACTIVATE";
                }
                else
                {
                    button.IsChecked = false;
                    ActivateButtonLabel = "ACTIVATE";
                }
            }
            else
            {
                profile.LastSectorName = null;
                profile.LastSectorFreq = null;

                SetProfileName();
                RadarViewModel.UpdateVatsimDataService();

                ActivateButtonLabel = "ACTIVATE";
            }
        }

        public void SetProfileName()
        {
            if (profile.LastSectorName != null)
            {
                ProfileName = $"vFalcon : {profile.Name} : {profile.LastSectorName}";
                return;
            }
            ProfileName = $"vFalcon : {profile.Name}";
        }

        private void InitializeMapDictionaries()
        {
            mapEnabledGetters = new()
            {
                ["BOUNDARY"] = () => IsBndryEnabled,
                ["APPROACH_CONTROL"] = () => IsAppchEnabled,
                ["LOW_SECTORS"] = () => IsLowsEnabled,
                ["HIGH_SECTORS"] = () => IsHighsEnabled
            };

            mapEnabledSetters = new()
            {
                ["BOUNDARY"] = val => profile.BndryEnabled = val,
                ["APPROACH_CONTROL"] = val => profile.AppchCntlEnabled = val,
                ["LOW_SECTORS"] = val => profile.LowsEnabled = val,
                ["HIGH_SECTORS"] = val => profile.HighsEnabled = val
            };
        }

        private void InitializeProfile()
        {
            CursorSize = profile.CursorSize ?? "2"; // default
            IsBndryEnabled = profile.BndryEnabled;
            IsAppchEnabled = profile.AppchCntlEnabled;
            IsLowsEnabled = profile.LowsEnabled;
            IsHighsEnabled = profile.HighsEnabled;
            var mapsToLoad = new (string Key, bool Enabled)[]
            {
                    ("BOUNDARY", IsBndryEnabled),
                    ("APPROACH_CONTROL", IsAppchEnabled),
                    ("LOW_SECTORS", IsLowsEnabled),
                    ("HIGH_SECTORS", IsHighsEnabled)
            };

            foreach (var (key, enabled) in mapsToLoad)
            {
                if (enabled)
                    ToggleVideoMap(key);
            }
        }

        private void StartZuluTimer()
        {
            ZuluTimerTick(null, null);
            zuluTimer.Interval = TimeSpan.FromMilliseconds(500);
            zuluTimer.Tick += ZuluTimerTick;
            zuluTimer.Start();
        }

        private void ZuluTimerTick(object? sender, EventArgs? e)
        {
            ZuluTime = DateTime.UtcNow.ToString("HHmm ss"); ;
        }

        private void DecreaseCursorSize()
        {
            CursorSize = CursorSize switch
            {
                "5" => "4",
                "4" => "3",
                "3" => "2",
                "2" => "1",
                _ => "1"
            };
            profile.CursorSize = CursorSize;
        }

        private void IncreaseCursorSize()
        {
            CursorSize = CursorSize switch
            {
                "1" => "2",
                "2" => "3",
                "3" => "4",
                "4" => "5",
                _ => "5"
            };
            profile.CursorSize = CursorSize;
        }

        private void OnSwitchProfile()
        {
            RequestSwitchProfile?.Invoke();
        }

        private async void OnSaveProfile()
        {
            if (RequestConfirmation != null)
            {
                bool confirmed = await RequestConfirmation.Invoke($"Save profile: \"{profile.Name}\"");
                if (confirmed)
                {
                    await profileService.Save(profile);
                }
            }
        }

        private void OnSaveProfileAs()
        {
            RequestSaveProfileAs?.Invoke();
            SetProfileName();
        }

        private void ToggleVideoMap(string? mapKey)
        {
            if (string.IsNullOrEmpty(mapKey) || !mapEnabledGetters.ContainsKey(mapKey)) return;

            bool isEnabled = mapEnabledGetters[mapKey]();

            var file = Loader.LoadFile($"VideoMaps/{profile.ArtccId}", $"{mapKey}.geojson");
            if (isEnabled)
                RadarViewModel.LoadVideoMap(file, "#00FF00");
            else
                RadarViewModel.UnloadVideoMap(file);

            mapEnabledSetters[mapKey](isEnabled);
            RadarViewModel.InvalidateCanvas?.Invoke();
        }
    }
}
