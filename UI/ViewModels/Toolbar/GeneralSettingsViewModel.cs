using System.Windows;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Utils;
namespace vFalcon.UI.ViewModels.Toolbar;

public class GeneralSettingsViewModel : ViewModelBase
{
    public bool TopDownMode
    {
        get => App.Profile.GeneralSettings.TopDown;
        set
        {
            App.Profile.GeneralSettings.TopDown = value;
            App.MainWindowViewModel.ReloadEramFeatures();
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            OnPropertyChanged();
        }
    }

    public bool VideoMapPreProcess
    {
        get => App.Profile.GeneralSettings.VideoMapPreProcess;
        set
        {
            App.Profile.GeneralSettings.VideoMapPreProcess = value;
            if (value) App.MainWindowViewModel.ReloadFeatures();
            else App.MainWindowViewModel.ReloadEramFeatures(true);
        }
    }

    public bool AutoDatablock
    {
        get
        {
            OnPropertyChanged(nameof(AutoDatablockEnabled));
            return App.Profile.GeneralSettings.AutoDatablock;
        }
        set
        {
            App.Profile.GeneralSettings.AutoDatablock = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AutoDatablockEnabled));
            App.MainWindowViewModel.PilotService.ResetDatablocksToDefault();
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public Visibility AutoDatablockEnabled => App.Profile.GeneralSettings.AutoDatablock ? Visibility.Collapsed : Visibility.Visible;

    public int SelectedDefaultDatablockType
    {
        get => (int)App.Profile.GeneralSettings.DefaultDatablockType;
        set
        {
            App.Profile.GeneralSettings.DefaultDatablockType = (DatablockType)value;
            App.MainWindowViewModel.PilotService.ResetDatablocksToDefault();
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            OnPropertyChanged();
        }
    }

    public int SelectedLogLevel
    {
        get => App.Profile.GeneralSettings.LogLevel;
        set
        {
            Logger.LogLevelThreshold = (LogLevel)value;
            App.Profile.GeneralSettings.LogLevel = value;
            OnPropertyChanged();
        }
    }
}
