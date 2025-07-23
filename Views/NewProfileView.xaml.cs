using AdonisUI.Controls;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for NewProfileView.xaml
    /// </summary>
    public partial class NewProfileView : AdonisWindow
    {
        public NewProfileView()
        {
            InitializeComponent();
            var viewModel = new NewProfileViewModel();
            DataContext = viewModel;
            viewModel.Close += () => this.Close();
        }
    }
}
