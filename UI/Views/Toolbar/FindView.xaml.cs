using AdonisUI.Controls;
using System.Globalization;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Toolbar;

public partial class FindView : AdonisWindow
{
    public FindView()
    {
        InitializeComponent();
        LoadWindowSettings();
        DataContext = new FindViewModel();
        SizeChanged += OnSizeChanged;
        LocationChanged += OnLocationChanged;
    }

    private void LoadWindowSettings()
    {
        double[] parts = App.Profile.FindSettings.WindowSettings.Bounds.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
        Left = parts[0];
        Top = parts[1];
        Width = parts[2];
        Height = parts[3];
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        App.Profile.FindSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        App.Profile.FindSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
    }
}
