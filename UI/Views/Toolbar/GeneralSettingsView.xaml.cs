using AdonisUI.Controls;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Tooolbar;
public partial class GeneralSettingsView : AdonisWindow
{
    public GeneralSettingsView()
    {
        InitializeComponent();
        DataContext = new GeneralSettingsViewModel();
    }
}
