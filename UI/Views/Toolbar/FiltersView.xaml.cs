using AdonisUI.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Toolbar
{
    public partial class FiltersView : AdonisWindow
    {
        FiltersViewModel FiltersViewModel { get; set; }
        public FiltersView()
        {
            InitializeComponent();
            LoadWindowSettings();
            FiltersViewModel = new();
            DataContext = FiltersViewModel;
            SizeChanged += OnSizeChanged;
            LocationChanged += OnLocationChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FiltersViewModel.SaveFiltersVisibility = Visibility.Visible;
        }

        private void LoadWindowSettings()
        {
            double[] parts = App.Profile.FilterSettings.WindowSettings.Bounds.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            Left = parts[0];
            Top = parts[1];
            if (parts[2] == -1) Width = double.NaN;
            else Width = parts[2];
            if (parts[3] == -1) Height = double.NaN;
            else Height = parts[3];
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            App.Profile.FilterSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
        }

        private void OnLocationChanged(object? sender, EventArgs e)
        {
            App.Profile.FilterSettings.WindowSettings.Bounds = $"{this.Left},{this.Top},{this.Width},{this.Height}";
        }
    }
}
