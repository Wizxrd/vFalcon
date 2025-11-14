using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using vFalcon.Mvvm;
using vFalcon.Services;
using vFalcon.Services.Interfaces;
using vFalcon.Utils;
using Message = vFalcon.Utils.Message;

namespace vFalcon.UI.ViewModels.Common;

public class NewProfileViewModel : ViewModelBase
{
    private JObject childFacilityTypes = new JObject();
    private ObservableCollection<string> installedArtccs = new();
    private readonly IProfileService profileService = new ProfileService();
    private string selectedArtcc = string.Empty;
    private string profileName = string.Empty;

    public ICommand CreateProfileCommand { get; }
    public ICommand CancelCommand { get; }

    public event Action? Close;

    public ObservableCollection<string> InstalledArtccs
    {
        get => installedArtccs;
        set
        {
            installedArtccs = value;
            OnPropertyChanged();
        }
    }

    public string SelectedArtcc
    {
        get => selectedArtcc;
        set
        {
            selectedArtcc = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsProfileNameable));
        }
    }

    public string ProfileName
    {
        get => profileName;
        set
        {
            if (profileName == value) return;
            profileName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsProfileCreatable));
        }
    }

    public bool IsProfileNameable => !string.IsNullOrEmpty(SelectedArtcc);
    public bool IsProfileCreatable => !string.IsNullOrEmpty(ProfileName);

    public NewProfileViewModel()
    {
        AddInstalledArtccs();
        CreateProfileCommand = new RelayCommand(OnCreateProfileCommand);
        CancelCommand = new RelayCommand(() => Close?.Invoke());
    }

    private async void AddInstalledArtccs()
    {
        InstalledArtccs = new ObservableCollection<string>();
        var files = Directory.GetFiles(PathFinder.GetFolderPath("ARTCCs"), "*.json");
        if (files.Length > 0)
        {
            foreach (var file in files)
            {
                string artccId = Path.GetFileNameWithoutExtension(file);
                var jsonText = await File.ReadAllTextAsync(PathFinder.GetFilePath($"ARTCCs", $"{artccId}.json"));
                var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonText);
                string name = (string)jsonObject["facility"]["name"];
                InstalledArtccs.Add($"{name} - {artccId}");            }
        }
        else
        {
            Message.Error("No ARTCCs are installed");
        }
    }

    private static string GetArtccId(string selectedArtcc)
    {
        if (string.IsNullOrWhiteSpace(selectedArtcc)) return string.Empty;
        int hyphenIndex = selectedArtcc.LastIndexOf('-');
        return hyphenIndex >= 0 ? selectedArtcc[(hyphenIndex + 1)..].Trim() : selectedArtcc.Trim();
    }

    private async void OnCreateProfileCommand()
    {
        if (string.IsNullOrEmpty(ProfileName)) return;
        string artccId = GetArtccId(selectedArtcc);
        await profileService.New(ProfileName, artccId);
        Close?.Invoke();
    }
}
