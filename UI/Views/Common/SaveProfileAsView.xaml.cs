using AdonisUI.Controls;
using vFalcon.UI.ViewModels.Common;
namespace vFalcon.UI.Views.Common;

public partial class SaveProfileAsView : AdonisWindow
{
    public SaveProfileAsView()
    {
        InitializeComponent();
        SaveProfileAsViewModel saveProfileAsViewModel = new();
        DataContext = saveProfileAsViewModel;
        saveProfileAsViewModel.Close += this.Close;
    }
}
