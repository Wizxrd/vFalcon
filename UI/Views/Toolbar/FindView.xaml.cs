using AdonisUI.Controls;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Toolbar;

public partial class FindView : AdonisWindow
{
    public FindView()
    {
        InitializeComponent();
        DataContext = new FindViewModel();
    }
}
