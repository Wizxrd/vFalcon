using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using vFalcon.Models;
using vFalcon.Views;
using vFalcon.Services;
using vFalcon.Services.Interfaces;

namespace vFalcon.ViewModels
{
    public class LoadProfileViewModel : ViewModelBase
    {
        private readonly IProfileService profileService = new ProfileService();

        private DateTime lastClickTime = DateTime.MinValue;
        private readonly TimeSpan doubleClickTimeSpan = TimeSpan.FromMilliseconds(250);

        private string _searchQuery;
        private Profile _selectedProfile;
        private int _selectedIndex = -1;
        public ProfileViewModel? LastSelectedProfileVM { get; set; }

        public event Action RequestOpenMainWindow;
        public event Func<Task<NewProfileResult>> RequestNewProfileWindow;
        public event Func<string, Task<bool>> RequestConfirmation;
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

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged();

                    if (_selectedIndex >= 0 && _selectedIndex < FilteredProfiles.Count)
                    {
                        var selected = FilteredProfiles[_selectedIndex];
                        HandleProfileSelection(selected, userInitiated: false); // prevent double click logic
                    }
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
            LastSelectedProfileVM = null;

            var loadedProfiles = profileService.LoadProfiles();
            foreach (var profile in loadedProfiles)
            {
                var viewModel = new ProfileViewModel(profile);
                Profiles.Add(viewModel);
            }
            FilterProfiles();
            if (FilteredProfiles.Count > 0)
            {
                var first = FilteredProfiles[0];
                HandleProfileSelection(first, userInitiated: false);
                SelectedIndex = 0;
            }
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
                    await profileService.Rename(profile.OriginalName, profile.Name);
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
                if (textBox.Text == string.Empty)
                {
                    profile.IsRenaming = false;
                    profile.Name = profile.OriginalName;
                    return;
                }
                await profileService.Rename(profile.OriginalName, textBox.Text);
                profile.IsRenaming = false;
                LoadProfiles();
            }
        }

        private async void OnCopyProfile(object obj)
        {
            if (obj is ProfileViewModel profile)
            {
                await profileService.Copy(profile.Model);
                LoadProfiles();
            }
        }

        private void OnExportProfile(object obj)
        {
            if (obj is ProfileViewModel profile)
            {
                profileService.Export(profile.Model);
                LoadProfiles();
            }
        }

        private async void OnDeleteProfile(object obj)
        {
            if (obj is ProfileViewModel profile)
            {
                if (RequestConfirmation != null)
                {
                    bool confirmed = await RequestConfirmation.Invoke($"Are you sure you want to delete profile: \"{profile.Name}\"");
                    if (confirmed)
                    {
                        await profileService.Delete(profile.Model);
                        LoadProfiles();
                    }
                }
            }
        }

        private void OnProfileSelected(object obj)
        {
            if (obj is ProfileViewModel selected)
            {
                HandleProfileSelection(selected, userInitiated: true);
            }
        }

        public void HandleProfileSelection(ProfileViewModel selected, bool userInitiated)
        {
            DateTime now = DateTime.Now;

            foreach (var profile in FilteredProfiles)
            {
                profile.IsSelected = false;
                selected.IsSelected = true;
                SelectedProfile = selected.Model;
                LastSelectedProfileVM = selected;
            }

            if (userInitiated && (now - lastClickTime) <= doubleClickTimeSpan)
            {
                RequestOpenMainWindow?.Invoke();
            }

            lastClickTime = now;
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

        private async void OnNewProfile()
        {
            if (RequestNewProfileWindow != null)
            {
                NewProfileResult result = await RequestNewProfileWindow.Invoke();
                if (result.WasCreated)
                {
                    LoadProfiles();
                    SelectedProfile = Profiles.FirstOrDefault(p => p.Name == result.Name)?.Model;
                    RequestOpenMainWindow?.Invoke();
                }
            }
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
                    await profileService.Import(file);
                    LoadProfiles();
                }
            }
        }
    }
}
