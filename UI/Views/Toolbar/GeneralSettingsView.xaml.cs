using AdonisUI.Controls;
using System.Globalization;
using System.Windows;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Tooolbar;
public partial class GeneralSettingsView : AdonisWindow
{
    public GeneralSettingsView()
    {
        InitializeComponent();
        LoadWindowSettings();
        DataContext = new GeneralSettingsViewModel();
        SizeChanged += OnSizeChanged;
        LocationChanged += OnLocationChanged;
    }

    private void LoadWindowSettings()
    {
        double[] parts = App.Profile.GeneralSettings.WindowSettings.Bounds.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        Left = parts[0];
        Top = parts[1];
        Width = parts[2];
        Height = parts[3];
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        App.Profile.GeneralSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        App.Profile.GeneralSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }
}
