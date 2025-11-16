using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Interfaces;
using vFalcon.Services.Service;
using vFalcon.Views;
using MessageBox = vFalcon.Services.Service.MessageBoxService;

namespace vFalcon.ViewModels
{
    public class LoadProfileViewModel : ViewModelBase
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        public IProfileService profileService = new ProfileService();
        private readonly IArtccService artccService = new ArtccService();
        private DateTime lastClickTime = DateTime.MinValue;
        private readonly TimeSpan doubleClickTimeSpan = TimeSpan.FromMilliseconds(250);

        private string _searchQuery;
        private Profile _selectedProfile;
        private int _selectedIndex = -1;
        private bool _isProfileSelected;

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

        public bool IsProfileSelected
        {
            get => _isProfileSelected;
            set { _isProfileSelected = value; OnPropertyChanged(); }
        }

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
        public event Action? OpenNewProfileView;
        public event Action? OpenManageArtccsView;

        // ========================================================
        //                      COMMANDS
        // ========================================================
        public ICommand SelectProfileCommand { get; }
        public ICommand NewProfileCommand { get; }
        public ICommand ImportProfileCommand { get; }
        public ICommand LoadProfileCommand { get; }
        public ICommand RenameProfileCommand { get; }
        public ICommand StopRenamingCommand { get; }
        public ICommand CopyProfileCommand { get; }
        public ICommand ExportProfileCommand { get; }
        public ICommand DeleteProfileCommand { get; }

        public ICommand OpenManageArtccsCommand { get; }
        public ICommand ImportArtccCommand { get; }

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public LoadProfileViewModel()
        {
            LoadProfiles();

            SelectProfileCommand = new RelayCommand(OnProfileSelected);
            NewProfileCommand = new RelayCommand(OnNewProfileCommand);
            ImportProfileCommand = new RelayCommand(OnImportProfileCommand);
            LoadProfileCommand = new RelayCommand(OnLoadProfileCommand);
            RenameProfileCommand = new RelayCommand(OnRenameProfileCommand);
            StopRenamingCommand = new RelayCommand(OnStopRenamingCommand);
            CopyProfileCommand = new RelayCommand(OnCopyProfileCommand);
            ExportProfileCommand = new RelayCommand(OnExportProfile);
            DeleteProfileCommand = new RelayCommand(OnDeleteProfile);
            OpenManageArtccsCommand = new RelayCommand(OnOpenManageArtccsCommand);
            ImportArtccCommand = new RelayCommand(OnImportArtccCommand);
        }

        // ========================================================
        //              COMMAND HANDLER METHODS
        // ========================================================

        private async void OnImportArtccCommand()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Import ARTCC",
                Filter = "JSON (*.json|*.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            };
            if (dialog.ShowDialog() == true)
            {
                string json = File.ReadAllText(dialog.FileName);
                Artcc? imported = JsonConvert.DeserializeObject<Artcc>(json);

                if (imported != null)
                {
                    string filePath = Path.Combine(Loader.LoadFolder("ARTCCs"), $"{imported.id}.json");
                    string serialized = JsonConvert.SerializeObject(imported, Formatting.Indented);
                    await File.WriteAllTextAsync(filePath, serialized);
                }
            }
        }

        private void OnOpenManageArtccsCommand()
        {
            OpenManageArtccsView?.Invoke();
        }

        private void OnProfileSelected(object obj)
        {
            if (obj is ProfileViewModel selected)
                HandleProfileSelection(selected, userInitiated: true);
        }

        private void OnNewProfileCommand() => OpenNewProfileView?.Invoke();

        private async void OnImportProfileCommand()
        {
            var imported = await profileService.Import();
            if (imported) LoadProfiles();
        }

        private async void OnLoadProfileCommand()
        {
            SelectedProfile.LastUsedAt = DateTime.UtcNow;
            await profileService.SaveAsync(SelectedProfile);
            OpenEramWindow?.Invoke();
        }

        private async void OnRenameProfileCommand()
        {
            if (LastSelectedProfileVM == null || !LastSelectedProfileVM.IsRenaming)
            {
                LastSelectedProfileVM?.BeginRename();
                return;
            }

            LastSelectedProfileVM.IsRenaming = false;

            if (LastSelectedProfileVM.Name != LastSelectedProfileVM.OriginalName)
            {
                await profileService.Rename(LastSelectedProfileVM.OriginalName, LastSelectedProfileVM.Name);
                LoadProfiles();
            }
        }

        private async void OnStopRenamingCommand(object obj)
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

            if (profile.Name == profile.OriginalName)
            {
                profile.IsRenaming = false;
                return;
            }

            await profileService.Rename(profile.OriginalName, textBox.Text);
            profile.IsRenaming = false;
            LoadProfiles();
        }

        private async void OnCopyProfileCommand()
        {
            if (SelectedProfile == null) return;
            await profileService.Copy(SelectedProfile);
            LoadProfiles();
        }

        private void OnExportProfile(object obj)
        {
            if (SelectedProfile == null) return;
            profileService.Export(SelectedProfile);
        }

        private async void OnDeleteProfile(object obj)
        {
            if (SelectedProfile == null) return;

            var confirmed = MessageBox.Confirm($"Are you sure you want to delete profile: \"{SelectedProfile.Name}\"");
            if (!confirmed) return;

            await profileService.Delete(SelectedProfile);
            LoadProfiles();
        }

        // ========================================================
        //                  MAIN METHODS
        // ========================================================

        public async void LoadProfiles()
        {
            Profiles.Clear();
            FilteredProfiles.Clear();
            UnselectProfiles();
            LastSelectedProfileVM = null;

            var loadedProfiles = await profileService.LoadProfiles();
            if (!loadedProfiles.Any()) return;

            foreach (var profile in loadedProfiles)
            {
                if (File.Exists(Loader.LoadFile($"ARTCCs", $"{profile.ArtccId}.json")))
                {
                    Profiles.Add(new ProfileViewModel(profile));
                }
            }

            var sorted = Profiles.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList();
            Profiles.Clear();
            foreach (var p in sorted)
                Profiles.Add(p);
            FilterProfiles();

            int mostRecentIndex = -1;
            DateTime latest = DateTime.MinValue;

            for (int i = 0; i < FilteredProfiles.Count; i++)
            {
                DateTime when = FilteredProfiles[i].Model.LastUsedAt;
                if (when > latest)
                {
                    latest = when;
                    mostRecentIndex = i;
                }
            }

            if (mostRecentIndex >= 0)
            {
                SelectedIndex = mostRecentIndex;
                HandleProfileSelection(FilteredProfiles[mostRecentIndex], false);
            }
        }

        private void UnselectProfiles()
        {
            IsProfileSelected = false;
            foreach (var profile in FilteredProfiles)
            {
                profile.IsSelected = false;
                break;
            }
        }

        public async void HandleProfileSelection(ProfileViewModel selected, bool userInitiated)
        {
            DateTime now = DateTime.Now;
            UnselectProfiles();
            foreach (var profile in FilteredProfiles)
                profile.IsSelected = false;

            selected.IsSelected = true;
            IsProfileSelected = true;
            SelectedProfile = selected.Model;
            LastSelectedProfileVM = selected;
            SelectedProfileArtcc = await artccService.LoadArtcc(SelectedProfile.ArtccId);
            if (userInitiated && (now - lastClickTime) <= doubleClickTimeSpan)
                OnLoadProfileCommand();

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
