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
        double[] parts = App.Profile.WindowSettings.Bounds.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        Rect bounds = new Rect(parts[0], parts[1], parts[2], parts[3]);
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        if (App.Profile.WindowSettings.IsMaximized)
        {
            WindowState = WindowState.Maximized;
        }
        if (App.Profile.WindowSettings.IsFullscreen)
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
        if (App.Profile.WindowSettings.ShowTitleBar)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
        }
        else
        {
            WindowStyle = WindowStyle.None;
        }
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        App.Profile.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        App.Profile.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        PilotContextPopup.IsOpen = false;
        App.Profile.WindowSettings.IsMaximized = (WindowState == WindowState.Maximized);
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