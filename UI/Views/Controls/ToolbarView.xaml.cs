using System.Windows;
using System.Windows.Controls;
using vFalcon.UI.ViewModels.Controls;
using vFalcon.UI.Views.Common;
using vFalcon.UI.Views.Manager;
using vFalcon.UI.Views.Toolbar;
using vFalcon.UI.Views.Tooolbar;
namespace vFalcon.UI.Views.Controls;

public partial class ToolbarView : UserControl
{
    private ViewManager? viewManager;
    public ToolbarViewModel? ViewModel { get; set; }
    public PositionsView? PositionsView { get; set; }
    public MapsView? MapsView { get; set; }
    public GeneralSettingsView? GeneralSettingsView { get; set; }
    public AppearanceSettingsView? AppearanceSettingsView { get; set; }
    public AircraftListView? AircraftListView { get; set; }
    public SaveProfileAsView? SaveProfileAsView { get; set; }
    public ToolbarView()
    {
        InitializeComponent();
        ViewModel = new();
        viewManager = App.ViewManager ?? new();
        ViewModel.OpenPositionsView += viewManager.OpenPositionsView;
        ViewModel.OpenMapsView += viewManager.OpenMapsView;
        ViewModel.OpenGeneralSettingsView += viewManager.OpenGeneralSettingsView;
        ViewModel.OpenAppearanceSettingsView += viewManager.OpenAppearanceSettingsView;
        ViewModel.OpenAircraftListView += viewManager.OpenAircraftListView;
        ViewModel.OpenSaveProfileAsView += viewManager.OpenSaveProfileAsView;
        ViewModel.OpenLoadProfileView += viewManager.OpenLoadProfileView;
        DataContext = ViewModel; 
    }
}
