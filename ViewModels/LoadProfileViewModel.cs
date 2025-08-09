using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Models;
using vFalcon.Services.Interfaces;
using vFalcon.Services.Service;

namespace vFalcon.ViewModels
{
    public class LoadProfileViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly IProfileService profileService = new ProfileService();
        private readonly IArtccService artccService = new ArtccService();
        private DateTime lastClickTime = DateTime.MinValue;
        private readonly TimeSpan doubleClickTimeSpan = TimeSpan.FromMilliseconds(250);

        private string _searchQuery;
        private Profile _selectedProfile;
        private int _selectedIndex = -1;

        // ========================================================
        //                  COLLECTIONS
        // ========================================================
        public ObservableCollection<ProfileViewModel> Profiles { get; } = new();
        public ObservableCollection<ProfileViewModel> FilteredProfiles { get; } = new();

        // ========================================================
        //                  PROPERTIES
        // ========================================================
        public ProfileViewModel? LastSelectedProfileVM { get; set; }
        public Profile? LastSelectedProfile { get; set; }
        public Artcc SelectedProfileArtcc { get; set; }

        public Profile SelectedProfile
        {
            get => _selectedProfile;
            set { _selectedProfile = value; OnPropertyChanged(); }
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
                        HandleProfileSelection(FilteredProfiles[_selectedIndex], userInitiated: false);
                }
            }
        }

        // ========================================================
        //                      EVENTS
        // ========================================================
        public event Action OpenEramWindow;
        public event Func<string, Task<bool>> RequestConfirmation;

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand SelectProfileCommand { get; }
        public ICommand RenameProfileCommand { get; }
        public ICommand StopRenamingCommand { get; }
        public ICommand CopyProfileCommand { get; }
        public ICommand ExportProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public LoadProfileViewModel()
        {
            LoadProfiles();

            SelectProfileCommand = new RelayCommand(OnProfileSelected);
            RenameProfileCommand = new RelayCommand(OnRenameProfile);
            StopRenamingCommand = new RelayCommand(OnStopRenaming);
            CopyProfileCommand = new RelayCommand(OnCopyProfile);
            ExportProfileCommand = new RelayCommand(OnExportProfile);
            DeleteProfileCommand = new RelayCommand(OnDeleteProfile);
        }

        // ========================================================
        //              COMMAND HANDLER METHODS
        // ========================================================
        private void OnProfileSelected(object obj)
        {
            if (obj is ProfileViewModel selected)
                HandleProfileSelection(selected, userInitiated: true);
        }

        private async void OnRenameProfile(object obj)
        {
            if (obj is not ProfileViewModel profile) return;

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

        private async void OnStopRenaming(object obj)
        {
            if (obj is not TextBox textBox) return;
            var profile = Profiles.FirstOrDefault(p => p.IsRenaming);
            if (profile == null) return;

            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                profile.IsRenaming = false;
                profile.Name = profile.OriginalName;
                return;
            }

            await profileService.Rename(profile.OriginalName, textBox.Text);
            profile.IsRenaming = false;
            LoadProfiles();
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
            if (obj is not ProfileViewModel profile) return;

            if (RequestConfirmation != null)
            {
                bool confirmed = await RequestConfirmation.Invoke(
                    $"Are you sure you want to delete your CRC profile: \"{profile.Name}\"");

                if (confirmed)
                {
                    await profileService.Delete(profile.Model);
                    LoadProfiles();
                }
            }
        }

        // ========================================================
        //                  MAIN METHODS
        // ========================================================
        private async void LoadProfiles()
        {
            Profiles.Clear();
            FilteredProfiles.Clear();
            LastSelectedProfileVM = null;

            var loadedProfiles = await profileService.LoadProfiles();
            if (!loadedProfiles.Any()) return;

            foreach (var profile in loadedProfiles)
                Profiles.Add(new ProfileViewModel(profile));

            FilterProfiles();

            if (FilteredProfiles.Count > 0)
            {
                var first = FilteredProfiles[0];
                HandleProfileSelection(first, userInitiated: false);
                SelectedIndex = 0;
            }
        }

        public async void HandleProfileSelection(ProfileViewModel selected, bool userInitiated)
        {
            DateTime now = DateTime.Now;

            foreach (var profile in FilteredProfiles)
                profile.IsSelected = false;

            selected.IsSelected = true;
            SelectedProfile = selected.Model;
            LastSelectedProfileVM = selected;
            SelectedProfileArtcc = await artccService.LoadArtcc(SelectedProfile.ArtccId);

            if (userInitiated && (now - lastClickTime) <= doubleClickTimeSpan)
                OpenEramWindow?.Invoke();

            lastClickTime = now;
        }

        private void FilterProfiles()
        {
            FilteredProfiles.Clear();
            string query = SearchQuery?.ToLower() ?? string.Empty;

            foreach (var profile in Profiles)
            {
                if (string.IsNullOrWhiteSpace(query) || profile.Name.ToLower().Contains(query))
                    FilteredProfiles.Add(profile);
            }
        }
    }
}
