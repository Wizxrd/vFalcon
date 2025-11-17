using AdonisUI.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using vFalcon.UI.ViewModels;
using vFalcon.Utils;
namespace vFalcon.UI.Views;

public partial class MainWindowView : AdonisWindow
{
    public MainWindowView()
    {
        InitializeComponent();
        Init();
        App.MainWindowView = this;
        DataContext = new MainWindowViewModel(DisplayControlView);
        SizeChanged += OnSizeChanged;
        LocationChanged += OnLocationChanged;
        StateChanged += OnStateChanged;
        Deactivated += OnDeactivated;
        Closing += OnClosing;
    }

    private void Init()
    {
        LoadProfile();
    }

    private void LoadProfile()
    {
        LoadWindowSettings();
    }

    private void LoadWindowSettings()
    {
        double[] parts = App.Profile.MainWindowSettings.WindowSettings.Bounds.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        Left = parts[0];
        Top = parts[1];
        if (parts[2] == -1) Width = double.NaN;
        else Width = parts[2];
        if (parts[3] == -1) Height = double.NaN;
        else Height = parts[3];
        if (App.Profile.MainWindowSettings.WindowSettings.IsMaximized) WindowState = WindowState.Maximized;
        if (App.Profile.MainWindowSettings.WindowSettings.IsFullscreen)
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Maximized;
        }
        else
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            ResizeMode = ResizeMode.CanResize;
            WindowState = WindowState.Normal;
        }
        if (App.Profile.MainWindowSettings.WindowSettings.ShowTitleBar) WindowStyle = WindowStyle.SingleBorderWindow;
        else WindowStyle = WindowStyle.None;
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        App.Profile.MainWindowSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        App.Profile.MainWindowSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        PilotContextPopup.IsOpen = false;
        App.Profile.MainWindowSettings.WindowSettings.IsMaximized = (WindowState == WindowState.Maximized);
    }
    
    private void OnDeactivated(object? sender, EventArgs e)
    {
        PilotContextPopup.IsOpen = false;
    }

    private void OnClosing(object? sender, EventArgs e)
    {
        Logger.Debug("OnClosing", "Closing");
        App.MainWindowViewModel.Dispose();
        App.MainWindowViewModel = null;
        App.ViewManager.Dispose();
    }
}