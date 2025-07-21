using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Models;
using vFalcon.Services;
using vFalcon.Services.Interfaces;

namespace vFalcon.ViewModels
{
    public class SaveProfileAsViewModel : ViewModelBase
    {
        private readonly IProfileService profileService = new ProfileService();
        private Profile profile { get; set; }
        private string _profileName;
        public ICommand SaveProfileAsCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action Close;

        public string ProfileName
        {
            get => _profileName;
            set
            {
                if (_profileName != value)
                {
                    _profileName = value;
                    OnPropertyChanged();
                    ((RelayCommand)SaveProfileAsCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public SaveProfileAsViewModel(Profile profile)
        {
            this.profile = profile;
            SaveProfileAsCommand = new RelayCommand(OnSaveProfileAs, () => !string.IsNullOrWhiteSpace(ProfileName));
            CancelCommand = new RelayCommand(OnCancel);
        }

        private async void OnSaveProfileAs()
        {
            await profileService.SaveAs(profile, ProfileName);
            Close?.Invoke();
        }
        private void OnCancel()
        {
            Close?.Invoke();
        }
    }
}
