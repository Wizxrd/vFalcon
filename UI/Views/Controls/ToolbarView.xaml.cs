using System.Windows.Controls;
using vFalcon.UI.ViewModels.Controls;
using vFalcon.UI.Views.Common;
using vFalcon.UI.Views.Toolbar;
using vFalcon.UI.Views.Tooolbar;
namespace vFalcon.UI.Views.Controls;

public partial class ToolbarView : UserControl
{
    public ToolbarView()
    {
        InitializeComponent();
        DataContext = new ToolbarViewModel();
    }
}
