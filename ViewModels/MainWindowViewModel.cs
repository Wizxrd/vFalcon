using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using vFalcon.Models;
using vFalcon.Services.Interfaces;
using vFalcon.Services;
using System.Drawing.Text;

namespace vFalcon.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IProfileService profileService = new ProfileService();

        private string _zuluTime;
        private DispatcherTimer zuluTimer = new DispatcherTimer();

        private string _cursorSize;
        private string _profileName;

        public event Action? RequestSwitchProfile;
        public event Action? RequestSaveProfile;
        public Func<string, Task<bool>> RequestConfirmation;
        public event Action? RequestSaveProfileAs;

        public ICommand DecreaseCursorSizeCommand { get; }
        public ICommand IncreaseCursorSizeCommand { get; }
        public ICommand SwitchProfileCommand { get; }
        public ICommand SaveProfileCommand { get; }
        public ICommand SaveProfileAsCommand { get; }

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
        public string CursorSize
        {
            get => _cursorSize;
            set
            {
                if (_cursorSize != value)
                {
                    _cursorSize = value;
                    profile.CursorSize = value;
                    OnPropertyChanged();
                }
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


        public MainWindowViewModel(Profile profile)
        {
            StartZuluTimer();

            this.profile = profile;
            SetProfileName();
            CursorSize = profile.CursorSize ?? "2"; // default

            DecreaseCursorSizeCommand = new RelayCommand(DecreaseCursorSize);
            IncreaseCursorSizeCommand = new RelayCommand(IncreaseCursorSize);
            SwitchProfileCommand = new RelayCommand(OnSwitchProfile);
            SaveProfileCommand = new RelayCommand(OnSaveProfile);
            SaveProfileAsCommand = new RelayCommand(OnSaveProfileAs);
        }

        public void SetProfileName()
        {
            ProfileName = $"vFalcon : {profile.Name}";
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
    }
}
