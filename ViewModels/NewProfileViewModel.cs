using System.Collections.ObjectModel;
using System.Windows.Input;
using vFalcon.Models;

namespace vFalcon.ViewModels
{
    public class NewProfileViewModel : ViewModelBase
    {
        private string _selectedArtcc;
        private string _profileName;
        public bool WasCreated {  get; set; }
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
                OnPropertyChanged(); // Notify for SelectedArtcc

                IsProfileNameEnabled = !string.IsNullOrWhiteSpace(_selectedArtcc);
            }
        }

        private bool _isProfileNameEnabled;
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
                    ((RelayCommand)CreateCommand).RaiseCanExecuteChanged(); // ✅ Required
                }
            }
        }

        public NewProfileViewModel()
        {
            ArtccOptions = new ObservableCollection<string>
            {
                "Albuquerque ARTCC - ZAB",
                "Anchorage ARTCC - ZAN",
                "Atlanta ARTCC - ZTL",
                "Boston ARTCC - ZBW",
                "Chicago ARTCC - ZAU",
                "Cleveland ARTCC - ZOB",
                "Denver ARTCC - ZDV",
                "Fort Worth ARTCC - ZFW",
                "Guam CERAP- ZUA",
                "Honolulu Control Facility- ZHN",
                "Houston ARTCC - ZHU",
                "Indianapolis ARTCC - ZID",
                "Jacksonville ARTCC - ZJX",
                "Kansas City ARTCC - ZKC",
                "Los Angeles ARTCC - ZLA",
                "Memphis ARTCC - ZME",
                "Miami ARTCC - ZMA",
                "Minneapolis ARTCC - ZMP",
                "New York ARTCC - ZNY",
                "Oakland ARTCC - ZOA",
                "Salt Lake ARTCC - ZLC",
                "San Juan CERAP - ZSU",
                "Seattle ARTCC - ZSE",
                "Washington ARTCC - ZDC"
            };

            CreateCommand = new RelayCommand(OnCreate, () => !string.IsNullOrWhiteSpace(ProfileName));
            CancelCommand = new RelayCommand(OnCancel);
        }

        private async void OnCreate()
        {
            await Profile.New(SelectedArtcc, ProfileName);
            WasCreated = true;
            Close?.Invoke();
        }

        private void OnCancel()
        {
            Close?.Invoke();
        }
    }
}
