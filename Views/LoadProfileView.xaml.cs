using AdonisUI.Controls;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for LoadProfileView.xaml
    /// </summary>
    public partial class LoadProfileView : AdonisWindow
    {
        public LoadProfileView()
        {
            InitializeComponent();
            var viewModel = new LoadProfileViewModel();
            viewModel.Close += () => this.Close();
            DataContext = viewModel;
        }
    }
}
