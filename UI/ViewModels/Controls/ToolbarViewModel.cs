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
namespace vFalcon.UI.ViewModels.Controls;

public class ToolbarViewModel : ViewModelBase
{
    private string showLatLonText = "Show Lat/Lon";

    public string ShowLatLonText
    {
        get => showLatLonText;
        set
        {
            showLatLonText = value;
            OnPropertyChanged();
        }
    }

    public ICommand OpenPositionsCommand { get; set; }
    public ICommand OpenMapsCommand { get; set; }
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

    public Action? OpenPositionsView;
    public Action? OpenMapsView;
    public Action? OpenGeneralSettingsView;
    public Action? OpenAppearanceSettingsView;
    public Action? OpenAircraftListView;
    public Action? OpenSaveProfileAsView;
    public Action? OpenLoadProfileView;

    public ToolbarViewModel()
    {
        OpenPositionsCommand = new RelayCommand(OnOpenPositionsCommand);
        OpenMapsCommand = new RelayCommand(OnOpenMapsCommand);
        OpenGeneralSettingsCommand = new RelayCommand(OnOpenGeneralSettingsCommand);
        OpenAppearanceSettingsCommand = new RelayCommand(OnOpenAppearanceSettingsCommand);
        OpenAircraftListCommand = new RelayCommand(OnOpenAircraftListCommand);
        ShowLatLonCommand = new RelayCommand(OnShowLatLonCommand);
        CaptureScreenCommand = new RelayCommand(OnCaptureScreenCommand);
        OpenDiscordCommand = new RelayCommand(OnOpenDiscordCommand);
        OpenGithubCommand = new RelayCommand(OnOpenGithubCommand);
        SaveProfileCommand = new RelayCommand(OnSaveProfileCommand);
        SaveProfileAsCommand = new RelayCommand(OnSaveProfileAsCommand);
        SwitchProfileCommand = new RelayCommand(OnSwitchProfileCommand);
    }

    private void OnOpenPositionsCommand()
    {
        OpenPositionsView?.Invoke();
    }

    private void OnOpenMapsCommand()
    {
        OpenMapsView?.Invoke();
    }

    private void OnOpenGeneralSettingsCommand()
    {
        OpenGeneralSettingsView?.Invoke();
    }

    private void OnOpenAppearanceSettingsCommand()
    {
        OpenAppearanceSettingsView?.Invoke();
    }

    private void OnOpenAircraftListCommand()
    {
        OpenAircraftListView?.Invoke();
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
        OpenSaveProfileAsView?.Invoke();
    }

    private void OnSwitchProfileCommand()
    {
        OpenLoadProfileView?.Invoke();
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
