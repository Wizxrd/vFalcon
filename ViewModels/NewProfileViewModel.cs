using Microsoft.VisualBasic.Logging;
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
using System.Xml.Linq;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Services.Interfaces;
using vFalcon.Services.Service;

namespace vFalcon.ViewModels
{
    public class NewProfileViewModel : ViewModelBase
    {
        private ObservableCollection<string> installedArtccs;
        private ObservableCollection<string> artccFacilities;
        private ObservableCollection<string> displayTypes = new ObservableCollection<string>();
        private readonly IProfileService profileService = new ProfileService();
        private string profileName = string.Empty;
        private string selectedArtcc = string.Empty;
        private string selectedFacilty = string.Empty;
        private string selectedDisplayType = string.Empty;

        public event Action? Close;
        public ICommand CreateProfileCommand { get; }
        public ICommand CancelCommand { get; }

        public ObservableCollection<string> DisplayTypes
        {
            get => displayTypes;
            set
            {
                displayTypes = value;
                OnPropertyChanged();
            }
        }

        public string SelectedFacilty
        {
            get => selectedFacilty;
            set
            {
                selectedFacilty = value;
                DisplayTypes.Clear();
                DisplayTypes.Add(GetDisplayType(value));
                OnPropertyChanged(nameof(IsDisplayTypeSelectable));
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> ArtccFacilities
        {
            get => artccFacilities;
            set
            {
                artccFacilities = value;
                OnPropertyChanged();
            }
        }

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
                AddChildFacilities(GetArtccId(value));
                OnPropertyChanged(nameof(IsFacilitySelectable));
            }
        }

        public string SelectedDisplayType
        {
            get => selectedDisplayType;
            set
            {
                selectedDisplayType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProfileNameable));
            }
        }

        public bool IsFacilitySelectable => !string.IsNullOrEmpty(SelectedArtcc);
        public bool IsProfileNameable => !string.IsNullOrEmpty(SelectedDisplayType);
        public bool IsProfileCreatable => !string.IsNullOrEmpty(ProfileName);
        public bool IsDisplayTypeSelectable => !string.IsNullOrEmpty(selectedFacilty);
        private string DisplayType = string.Empty;
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

        private string GetDisplayType(string facility)
        {
            string facilityType = (string)ChildFacilityTypes[facility];
            string displayType = string.Empty;
            if (facilityType.Contains("Tracon")) displayType = "STARS";
            else if (facilityType == "Artcc") displayType = "ERAM";
            return displayType;
        }

        public NewProfileViewModel()
        {
            AddInstalledArtccs();
            CreateProfileCommand = new RelayCommand(OnCreateProfileCommand);
            CancelCommand = new RelayCommand(() => Close?.Invoke());
        }

        private JObject ChildFacilityTypes = new JObject();

        private async void AddChildFacilities(string artccId)
        {
            ArtccFacilities = new ObservableCollection<string>();
            ChildFacilityTypes = new JObject();
            var jsonText = await File.ReadAllTextAsync(Loader.LoadFile($"ARTCCs", $"{artccId}.json"));
            var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonText);
            string name = (string)jsonObject["facility"]["name"];
            JArray childFacilities = (JArray)jsonObject["facility"]["childFacilities"];
            ArtccFacilities.Add($"{artccId} - {name}");
            ChildFacilityTypes.Add($"{artccId} - {name}", (string)jsonObject["facility"]["type"]);
            foreach (JObject child in childFacilities)
            {
                string childId = (string)child["id"];
                string childName = (string)child["name"];
                string childType = (string)child["type"];
                ArtccFacilities.Add($"{childId} - {childName}");
                ChildFacilityTypes.Add($"{childId} - {childName}", childType);
            }
        }

        private async void AddInstalledArtccs()
        {
            InstalledArtccs = new ObservableCollection<string>();
            var files = Directory.GetFiles(Loader.LoadFolder("ARTCCs"), "*.json");
            foreach (var file in files)
            {
                string artccId = Path.GetFileNameWithoutExtension(file);
                var jsonText = await File.ReadAllTextAsync(Loader.LoadFile($"ARTCCs", $"{artccId}.json"));
                var jsonObject = JsonConvert.DeserializeObject<JObject>(jsonText);
                string name = (string)jsonObject["facility"]["name"];
                InstalledArtccs.Add($"{name} - {artccId}");
                AddChildFacilities(artccId);
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
            await profileService.New(ProfileName, artccId, SelectedFacilty.Substring(0,3), SelectedDisplayType);
            Close?.Invoke();
        }
    }
}
