using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Services;
using vFalcon.Services.Interfaces;
using vFalcon.Utils;
namespace vFalcon.UI.ViewModels.Common;

public class LoadProfileViewModel : ViewModelBase
{
    public IProfileService profileService = new ProfileService();
    private readonly IArtccService artccService = new ArtccService();
    private DateTime lastClickTime = DateTime.MinValue;
    private readonly TimeSpan doubleClickTimeSpan = TimeSpan.FromMilliseconds(250);

    private string searchQuery = string.Empty;
    private Profile selectedProfile;
    private int selectedIndex = -1;
    private bool isProfileSelected;

    public ObservableCollection<ProfileViewModel> Profiles { get; } = new();
    public ObservableCollection<ProfileViewModel> FilteredProfiles { get; } = new();

    public ProfileViewModel? LastSelectedProfileVM { get; set; }
    public Profile? LastSelectedProfile { get; set; }
    public Artcc SelectedProfileArtcc { get; set; }

    public bool IsProfileSelected
    {
        get => isProfileSelected;
        set { isProfileSelected = value; OnPropertyChanged(); }
    }

    public Profile SelectedProfile
    {
        get => selectedProfile;
        set { selectedProfile = value; OnPropertyChanged(); }
    }

    public string SearchQuery
    {
        get => searchQuery;
        set
        {
            if (searchQuery != value)
            {
                searchQuery = value;
                OnPropertyChanged();
                FilterProfiles();
            }
        }
    }

    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (selectedIndex != value)
            {
                selectedIndex = value;
                OnPropertyChanged();

                if (selectedIndex >= 0 && selectedIndex < FilteredProfiles.Count)
                    HandleProfileSelection(FilteredProfiles[selectedIndex], userInitiated: false);
            }
        }
    }

    public event Action? OpenMainWindowView;
    public event Action? OpenNewProfileView;
    public event Action? OpenManageArtccsView;

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
                string filePath = Path.Combine(PathFinder.GetFolderPath("ARTCCs"), $"{imported.id}.json");
                string serialized = JsonConvert.SerializeObject(imported, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, serialized);
            }
        }
    }

    private void OnOpenManageArtccsCommand()
    {
        OpenManageArtccsView?.Invoke();
        LoadProfiles();
    }

    private void OnProfileSelected(object obj)
    {
        if (obj is ProfileViewModel selected)
            HandleProfileSelection(selected, userInitiated: true);
    }

    private void OnNewProfileCommand()
    {
        var files = Directory.GetFiles(PathFinder.GetFolderPath("ARTCCs"), "*.json");
        if (files.Length == 0)
        {
            Message.Error("No ARTCCs are installed");
            return;
        }
        OpenNewProfileView?.Invoke();
        LoadProfiles();
    }

    private async void OnImportProfileCommand()
    {
        var imported = await profileService.Import();
        if (imported) LoadProfiles();
    }

    private async void OnLoadProfileCommand()
    {
        SelectedProfile.LastUsedAt = DateTime.UtcNow;
        await profileService.SaveAsync(SelectedProfile);
        OpenMainWindowView?.Invoke();
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
        if (obj is not TextBox textBox)
        {
            OnLoadProfileCommand();
            return;
        }
        var profile = Profiles.FirstOrDefault(p => p.IsRenaming);
        if (profile == null) return;
        Logger.Debug("3", "3");
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

        var confirmed = Message.Confirm($"Are you sure you want to delete profile: \"{SelectedProfile.Name}\"");
        if (!confirmed) return;

        await profileService.Delete(SelectedProfile);
        LoadProfiles();
    }

    public async void LoadProfiles()
    {
        Profiles.Clear();
        FilteredProfiles.Clear();
        UnselectProfiles();
        LastSelectedProfileVM = null;

        var loadedProfiles = await profileService.GetProfiles();
        if (!loadedProfiles.Any()) return;

        foreach (var profile in loadedProfiles)
        {
            if (File.Exists(PathFinder.GetFilePath($"ARTCCs", $"{profile.ArtccId}.json")))
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
        SelectedProfileArtcc = await artccService.GetArtcc(SelectedProfile.ArtccId);
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
