using AdonisUI.Controls;
using vFalcon.UI.ViewModels.Toolbar;
namespace vFalcon.UI.Views.Toolbar;

public partial class AppearanceSettingsView : AdonisWindow
{
    public AppearanceSettingsView()
    {
        InitializeComponent();
        DataContext = new AppearanceSettingsViewModel();
    }
}
