using AdonisUI.Controls;
using System.Globalization;
using System.Windows;
using vFalcon.Models;
using vFalcon.Utils;
using vFalcon.UI.ViewModels;
using vFalcon.UI.ViewModels.Controls;
namespace vFalcon.UI.Views;

public partial class MainWindowView : AdonisWindow
{
    private Profile profile { get; set; } = App.Profile;
    private Artcc artcc { get; set; } = App.Artcc;

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
        double[] parts = profile.WindowSettings.Bounds.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        Rect bounds = new Rect(parts[0], parts[1], parts[2], parts[3]);
        Left = bounds.Left;
        Top = bounds.Top;
        Width = bounds.Width;
        Height = bounds.Height;
        if (profile.WindowSettings.IsMaximized)
        {
            WindowState = WindowState.Maximized;
        }
        if (profile.WindowSettings.IsFullscreen)
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
        if (profile.WindowSettings.ShowTitleBar)
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
        profile.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        profile.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        PilotContextPopup.IsOpen = false;
        profile.WindowSettings.IsMaximized = (WindowState == WindowState.Maximized);
    }
    
    private void OnDeactivated(object? sender, EventArgs e)
    {
        PilotContextPopup.IsOpen = false;
    }

    private void OnClosing(object? sender, EventArgs e)
    {
        Logger.Debug("OnClosing", "Closing");
        App.MainWindowViewModel.PilotService.Dispose();
        App.MainWindowViewModel.DatablockService.Dispose();
        App.MainWindowViewModel.EramFeatures = null;
        App.MainWindowViewModel.EramFeaturesCombined = null;
        App.MainWindowViewModel.ActiveEramMaps = null;
        App.MainWindowViewModel.StarsFeatures = null;
        App.MainWindowViewModel.RenderableFeatures = null;
        App.MainWindowViewModel.ActiveMaps = null;
        App.ViewManager.Dispose();
    }
}