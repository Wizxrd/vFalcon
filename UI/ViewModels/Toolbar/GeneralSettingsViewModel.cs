using System.Windows;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Utils;
namespace vFalcon.UI.ViewModels.Toolbar;

public class GeneralSettingsViewModel : ViewModelBase
{
    public bool TopDownMode
    {
        get => App.Profile.TopDown;
        set
        {
            App.Profile.TopDown = value;
            App.MainWindowViewModel.ReloadEramFeatures();
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            OnPropertyChanged();
        }
    }

    public bool VideoMapPreProcess
    {
        get => App.Profile.VideoMapPreProcess;
        set
        {
            App.Profile.VideoMapPreProcess = value;
            if (value) App.MainWindowViewModel.InitFeatures();
            else App.MainWindowViewModel.ReloadEramFeatures(true);
        }
    }

    public bool AutoDatablock
    {
        get
        {
            OnPropertyChanged(nameof(AutoDatablockEnabled));
            return App.Profile.AutoDatablock;
        }
        set
        {
            App.Profile.AutoDatablock = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AutoDatablockEnabled));
            App.MainWindowViewModel.PilotService.ResetDatablocksToDefault();
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public Visibility AutoDatablockEnabled => App.Profile.AutoDatablock ? Visibility.Collapsed : Visibility.Visible;

    public int SelectedDefaultDatablockType
    {
        get => (int)App.Profile.DefaultDatablockType;
        set
        {
            App.Profile.DefaultDatablockType = (DatablockType)value;
            App.MainWindowViewModel.PilotService.ResetDatablocksToDefault();
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
            OnPropertyChanged();
        }
    }

    public int SelectedLogLevel
    {
        get => App.Profile.LogLevel;
        set
        {
            Logger.LogLevelThreshold = (LogLevel)value;
            App.Profile.LogLevel = value;
            OnPropertyChanged();
        }
    }
}
