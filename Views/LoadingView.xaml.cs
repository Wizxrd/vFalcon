using AdonisUI.Controls;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for LoadingView.xaml
    /// </summary>
    public partial class LoadingView : AdonisWindow
    {
        public LoadingView()
        {
            InitializeComponent();
            LoadingViewModel loadingViewModel = new LoadingViewModel();
            loadingViewModel.Close = () => this.Close();
            DataContext = loadingViewModel;
        }
    }
}
