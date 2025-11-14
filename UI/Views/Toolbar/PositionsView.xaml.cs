using AdonisUI.Controls;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Toolbar;

public partial class PositionsView : AdonisWindow
{
    public PositionsView()
    {
        InitializeComponent();
        DataContext = new PositionsViewModel();
    }
}
