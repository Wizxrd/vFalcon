using AdonisUI.Controls;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Toolbar;

public partial class AircraftListView : AdonisWindow
{
    public AircraftListView()
    {
        InitializeComponent();
        DataContext = new AircraftListViewModel();
    }
}
