using AdonisUI.Controls;
using System.Windows.Input;
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
            DataContext = viewModel;
            viewModel.RequestConfirmation += ShowConfirmDialog;
        }

        private Task<bool> ShowConfirmDialog(string message)
        {
            var dialog = new ConfirmView(message)
            {
                Title = "Confirm",
                Owner = this
            };
            bool? result = dialog.ShowDialog();
            return Task.FromResult(result == true);
        }
    }
}
