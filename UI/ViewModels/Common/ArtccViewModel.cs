using vFalcon.Mvvm;
namespace vFalcon.UI.ViewModels.Common;

public class ArtccViewModel : ViewModelBase
{
    string name = string.Empty;
    string statusText = string.Empty;
    bool isInstalled;
    int profiles;
    string installUninstallText = string.Empty;
    string statusTextForeground = string.Empty;
    string artccStatusTextForeground = string.Empty;
    bool isInstalling = false;

    public string ArtccStatusTextForeground
    {
        get => artccStatusTextForeground;
        set
        {
            artccStatusTextForeground = value;
            OnPropertyChanged();
        }
    }

    public string StatusTextForeground
    {
        get => statusTextForeground;
        set
        {
            statusTextForeground = value;
            OnPropertyChanged();
        }
    }

    public string InstallUninstallText
    {
        get => installUninstallText;
        set
        {
            installUninstallText = value;
            OnPropertyChanged();
        }
    }
    public bool IsInstalling
    {
        get => isInstalling;
        set
        {
            isInstalling = value;
            OnPropertyChanged();
        }
    }
    public string Name
    {
        get => name;
        set
        {
            if (value == name) return;
            name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
        }
    }
    public bool IsInstalled
    {
        get => isInstalled;
        set
        {
            if (value == isInstalled) return;
            isInstalled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
        }
    }
    public int Profiles
    {
        get => profiles;
        set
        {
            if (value == profiles) return;
            profiles = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
        }
    }
    public string StatusText
    {
        get => statusText;
        set
        {
            statusText = value;
            OnPropertyChanged();
        }
    }
}
