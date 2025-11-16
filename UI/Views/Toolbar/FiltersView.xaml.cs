using AdonisUI.Controls;
using System.Windows.Controls;
using System.Windows;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Toolbar
{
    public partial class FiltersView : AdonisWindow
    {
        FiltersViewModel FiltersViewModel { get; set; }
        public FiltersView()
        {
            InitializeComponent();
            FiltersViewModel = new();
            DataContext = FiltersViewModel;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            FiltersViewModel.SaveFiltersVisibility = Visibility.Visible;
        }
    }
}
