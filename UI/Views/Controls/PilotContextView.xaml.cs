using System.Windows.Controls;
using vFalcon.UI.ViewModels.Controls;
namespace vFalcon.UI.Views.Controls;
public partial class PilotContextView : UserControl
{
    public PilotContextView()
    {
        InitializeComponent();
        DataContext = new PilotContextViewModel();
    }
}
