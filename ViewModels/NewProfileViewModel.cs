using System.Collections.ObjectModel;
using System.Windows.Input;
using vFalcon.Helpers;
using vFalcon.Services;
using vFalcon.Services.Interfaces;

namespace vFalcon.ViewModels
{
    public class NewProfileViewModel : ViewModelBase
    {
        private IProfileService profileService = new ProfileService();
        private IArtccService artccService = new ArtccService();

        private string _selectedArtcc;
        private bool _isProfileNameEnabled;
        private string _profileName;
        public bool WasCreated {  get; set; }
        public string NewName { get; set; }

        public ObservableCollection<string> ArtccOptions { get; }

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action Close;

        public string SelectedArtcc
        {
            get => _selectedArtcc;
            set
            {
                _selectedArtcc = value;
                OnPropertyChanged();

                IsProfileNameEnabled = !string.IsNullOrWhiteSpace(_selectedArtcc);
            }
        }

        public bool IsProfileNameEnabled
        {
            get => _isProfileNameEnabled;
            set
            {
                _isProfileNameEnabled = value;
                OnPropertyChanged();
            }
        }

        public string ProfileName
        {
            get => _profileName;
            set
            {
                if (_profileName != value)
                {
                    _profileName = value;
                    OnPropertyChanged();
                    ((RelayCommand)CreateCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public NewProfileViewModel()
        {
            ArtccOptions = new ObservableCollection<string>(artccService.GetArtccs());

            CreateCommand = new RelayCommand(OnCreate, () => !string.IsNullOrWhiteSpace(ProfileName));
            CancelCommand = new RelayCommand(OnCancel);
        }

        private async void OnCreate()
        {
            try
            {
                await profileService.New(SelectedArtcc, ProfileName);
                WasCreated = true;
                NewName = ProfileName;
                Close?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error("NewProfileViewModel.OnCreate", ex.ToString());
            }
        }

        private void OnCancel()
        {
            Close?.Invoke();
        }
    }
}
