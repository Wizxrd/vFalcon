using System.Windows.Input;
using vFalcon.Mvvm;
using vFalcon.Services;
using vFalcon.Utils;

namespace vFalcon.UI.ViewModels.Common;

public class SaveProfileAsViewModel : ViewModelBase
{
    private ProfileService profileService = new();
    private string profileName = string.Empty;
    private bool profileSaveable = false;

    public string ProfileName
    {
        get => profileName;
        set
        {
            profileName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProfileSaveable));
        }
    }

    public bool ProfileSaveable => !string.IsNullOrEmpty(ProfileName);

    public ICommand SaveAsCommand { get; set; }
    public ICommand CancelCommand { get; set; }

    public Action? Close;

    public SaveProfileAsViewModel()
    {
        SaveAsCommand = new RelayCommand(OnSaveAsCommand);
        CancelCommand = new RelayCommand(OnCancelCommand);
    }

    private async void OnSaveAsCommand()
    {
        if (ProfileSaveable)
        {
            await profileService.SaveAs(App.Profile, ProfileName);
            Close?.Invoke();
        }
    }

    private void OnCancelCommand()
    {
        Close?.Invoke();
    }
}
