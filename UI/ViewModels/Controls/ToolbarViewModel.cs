using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Services;
using vFalcon.Utils;
using Message = vFalcon.Utils.Message;
namespace vFalcon.UI.ViewModels.Controls;

public class ToolbarViewModel : ViewModelBase
{
    private string showLatLonText = "Show Lat/Lon";
    private bool exitReplayIsEnabled = false;

    public string ShowLatLonText
    {
        get => showLatLonText;
        set
        {
            showLatLonText = value;
            OnPropertyChanged();
        }
    }
    public bool ExitReplayIsEnabled
    {
        get => exitReplayIsEnabled;
        set
        {
            exitReplayIsEnabled = value;
            OnPropertyChanged();
        }
    }

    public ICommand OpenPositionsCommand { get; set; }
    public ICommand ToggleRecordingCommand { get; set; }
    public ICommand LoadReplayCommand { get; set; }
    public ICommand ExitReplayCommand { get; set; }
    public ICommand OpenMapsCommand { get; set; }
    public ICommand OpenFiltersCommand { get; set; }
    public ICommand OpenFindCommand { get; set; }
    public ICommand OpenGeneralSettingsCommand { get; set; }
    public ICommand OpenAppearanceSettingsCommand { get; set; }
    public ICommand OpenAircraftListCommand { get; set; }
    public ICommand ShowLatLonCommand { get; set; }
    public ICommand CaptureScreenCommand { get; set; }
    public ICommand SaveProfileCommand { get; set; }
    public ICommand SaveProfileAsCommand { get; set; }
    public ICommand SwitchProfileCommand { get; set; }
    public ICommand OpenDiscordCommand { get; set; }
    public ICommand OpenGithubCommand { get; set; }

    public ToolbarViewModel()
    {
        OpenPositionsCommand = new RelayCommand(OnOpenPositionsCommand);
        ToggleRecordingCommand = new RelayCommand(OnToggleRecordingCommand);
        LoadReplayCommand = new RelayCommand(OnLoadReplayCommand);
        ExitReplayCommand = new RelayCommand(OnExitReplayCommand);
        OpenMapsCommand = new RelayCommand(OnOpenMapsCommand);
        OpenFiltersCommand = new RelayCommand(OnOpenFiltersCommand);
        OpenFindCommand = new RelayCommand(OnOpenFindCommand);
        OpenGeneralSettingsCommand = new RelayCommand(OnOpenGeneralSettingsCommand);
        OpenAircraftListCommand = new RelayCommand(OnOpenAircraftListCommand);
        ShowLatLonCommand = new RelayCommand(OnShowLatLonCommand);
        OpenAppearanceSettingsCommand = new RelayCommand(OnOpenAppearanceSettingsCommand);
        CaptureScreenCommand = new RelayCommand(OnCaptureScreenCommand);
        OpenDiscordCommand = new RelayCommand(OnOpenDiscordCommand);
        OpenGithubCommand = new RelayCommand(OnOpenGithubCommand);
        SaveProfileCommand = new RelayCommand(OnSaveProfileCommand);
        SaveProfileAsCommand = new RelayCommand(OnSaveProfileAsCommand);
        SwitchProfileCommand = new RelayCommand(OnSwitchProfileCommand);
    }

    private void OnOpenPositionsCommand()
    {
        App.ViewManager.OpenPositionsView();
    }

    private void OnToggleRecordingCommand()
    {
        App.MainWindowViewModel.OnToggleRecordingCommand();
    }

    private void OnLoadReplayCommand()
    {
        App.MainWindowViewModel.OnLoadRecordingCommand();
    }

    private void OnExitReplayCommand()
    {
        ExitReplayIsEnabled = false;
        App.MainWindowViewModel.OnExitRecordingCommand();
    }

    private void OnOpenMapsCommand()
    {
        App.ViewManager.OpenMapsView();
    }

    private void OnOpenFiltersCommand()
    {
        App.ViewManager.OpenFiltersView();
    }

    private void OnOpenFindCommand()
    {
        App.ViewManager.OpenFindView();
    }

    private void OnOpenGeneralSettingsCommand()
    {
        App.ViewManager.OpenGeneralSettingsView();
    }

    private void OnOpenAppearanceSettingsCommand()
    {
        App.ViewManager.OpenAppearanceSettingsView();
    }

    private void OnOpenAircraftListCommand()
    {
        App.ViewManager.OpenAircraftListView();
    }

    private void OnShowLatLonCommand()
    {
        if (App.MainWindowViewModel.DisplayMouseCoordinates)
        {
            ShowLatLonText = "Show Lat/Lon";
            App.MainWindowViewModel.DisplayMouseCoordinates = false;
        }
        else
        {
            ShowLatLonText = "Hide Lat/Lon";
            App.MainWindowViewModel.DisplayMouseCoordinates = true;
        }
    }

    private void OnCaptureScreenCommand()
    {
        Screenshot.Capture();
    }

    private void OnSaveProfileCommand()
    {
        App.MainWindowViewModel.ProfileService.ConfirmSave();
    }

    private void OnSaveProfileAsCommand()
    {
        App.ViewManager.OpenSaveProfileAsView();
    }

    private void OnSwitchProfileCommand()
    {
        App.ViewManager.OpenLoadProfileView();
    }

    private void OnOpenDiscordCommand()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://discord.gg/8g7MFs5QfB",
            UseShellExecute = true
        });
    }

    private void OnOpenGithubCommand()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/Wizxrd/vFalcon",
            UseShellExecute = true
        });
    }
}
