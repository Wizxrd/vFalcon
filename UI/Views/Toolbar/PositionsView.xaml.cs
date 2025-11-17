using AdonisUI.Controls;
using System.Globalization;
using System.Windows;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Toolbar;

public partial class PositionsView : AdonisWindow
{
    public PositionsView()
    {
        InitializeComponent();
        LoadWindowSettings();
        DataContext = new PositionsViewModel();
        SizeChanged += OnSizeChanged;
        LocationChanged += OnLocationChanged;
    }

    private void LoadWindowSettings()
    {
        double[] parts = App.Profile.PositionsSettings.WindowSettings.Bounds.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        Left = parts[0];
        Top = parts[1];
        Width = parts[2];
        Height = parts[3];
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        App.Profile.PositionsSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        App.Profile.PositionsSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }
}
