using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using vFalcon.Models;
using vFalcon.Views;

namespace vFalcon.ViewModels
{
    public class LoadProfileViewModel : ViewModelBase
    {
        private string _searchQuery;
        private Profile _selectedProfile;
        public event Action Close;
        public ObservableCollection<ProfileViewModel> Profiles { get; } = new();
        public ObservableCollection<ProfileViewModel> FilteredProfiles { get; } = new();
        public ICommand NewProfileCommand { get; }
        public ICommand ImportProfileCommand { get; }
        public ICommand SelectProfileCommand { get; }
        
        public ICommand RenameProfileCommand { get; }
        public ICommand StopRenamingCommand { get; }
        public ICommand CopyProfileCommand { get; }
        public ICommand ExportProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }

        public Profile SelectedProfile
        {
            get => _selectedProfile;
            set
            {
                _selectedProfile = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    FilterProfiles();
                }
            }
        }

        public LoadProfileViewModel()
        {
            NewProfileCommand = new RelayCommand(OnNewProfile);
            ImportProfileCommand = new RelayCommand(OnImportProfile);
            SelectProfileCommand = new RelayCommand(OnProfileSelected);

            RenameProfileCommand = new RelayCommand(OnRenameProfile);
            StopRenamingCommand = new RelayCommand(OnStopRenaming);
            CopyProfileCommand = new RelayCommand(OnCopyProfile);
            ExportProfileCommand = new RelayCommand(OnExportProfile);
            DeleteProfileCommand = new RelayCommand(OnDeleteProfile);

            LoadProfiles();
        }

        private void LoadProfiles()
        {
            Profiles.Clear();
            FilteredProfiles.Clear();

            var loadedProfiles = Profile.LoadProfiles();
            foreach (var profile in loadedProfiles)
            {
                var viewModel = new ProfileViewModel(profile);
                Profiles.Add(viewModel);
            }
            FilterProfiles();
        }

        private async void OnRenameProfile(object obj)
        {
            if (obj is ProfileViewModel profile)
            {
                if (!profile.IsRenaming)
                {
                    profile.BeginRename();
                    return;
                }
                profile.IsRenaming = false;
                if (profile.Name != profile.OriginalName)
                {
                    await Profile.Rename(profile.OriginalName, profile.Name);
                    LoadProfiles();
                }
            }
        }

        private async void OnStopRenaming(object obj)
        {
            if (obj is not TextBox textBox) return;
            var profile = Profiles.FirstOrDefault(p => p.IsRenaming);
            if (profile != null)
            {
                await Profile.Rename(profile.OriginalName, textBox.Text);
                profile.IsRenaming = false;
                LoadProfiles();
            }
        }

        private async void OnCopyProfile(object obj)
        {
            if (obj is ProfileViewModel profile)
            {
                await Profile.Copy(profile.Model);
                LoadProfiles();
            }
        }

        private void OnExportProfile(object obj)
        {
            if (obj is ProfileViewModel profile)
            {
                Logger.Debug("Export", profile.Name);
                Profile.Export(profile.Model);
                LoadProfiles();
            }
        }

        private async void OnDeleteProfile(object obj)
        {
            if (obj is ProfileViewModel profile)
            {
                ConfirmView confirmView = new ConfirmView($"Are you sure you want to delete profile: \"{profile.Name}\"")
                {
                    Title = "Confirm Delete",
                    Owner = Application.Current.MainWindow
                };
                bool? result = confirmView.ShowDialog();

                if (result == true)
                {
                    await Profile.Delete(profile.Model);
                    LoadProfiles();
                }
            }
        }

        private DateTime lastClickTime = DateTime.MinValue;
        private readonly TimeSpan doubleClickTimeSpan = TimeSpan.FromMilliseconds(500);
        private void OnProfileSelected(object obj)
        {
            if (obj is ProfileViewModel selected)
            {
                DateTime now = DateTime.Now;

                foreach (var profile in FilteredProfiles)
                {
                    profile.IsSelected = false;
                }
                selected.IsSelected = true;
                SelectedProfile = selected.Model;
                if ((now - lastClickTime) <= doubleClickTimeSpan)
                {
                    Logger.Debug("Double", SelectedProfile.Name);
                    MainWindowView mainWindowView = new MainWindowView();
                    Close?.Invoke();
                    mainWindowView.ShowDialog();
                }
                lastClickTime = now;
            }
        }

        private void FilterProfiles()
        {
            FilteredProfiles.Clear();
            var query = SearchQuery?.ToLower() ?? string.Empty;

            foreach (var profile in Profiles)
            {
                if (string.IsNullOrWhiteSpace(query) || profile.Name.ToLower().Contains(query))
                {
                    FilteredProfiles.Add(profile);
                }
            }
        }

        private void OnNewProfile()
        {
            NewProfileView newProfileView = new NewProfileView();
            newProfileView.Owner = Application.Current.MainWindow;
            newProfileView.ShowDialog();
            LoadProfiles();
        }

        private async void OnImportProfile()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Import Profile",
                Filter = "JSON File (*.json)|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                string file = openFileDialog.FileName;
                if (file.Contains(".json"))
                {
                    await Profile.Import(file);
                    LoadProfiles();
                }
            }
        }
    }
}
