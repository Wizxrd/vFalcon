using AdonisUI.Controls;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for ConfirmView.xaml
    /// </summary>
    public partial class ConfirmView : AdonisWindow
    {
        public bool? DialogResultValue { get; private set; }

        public ConfirmView(string message)
        {
            InitializeComponent();

            var viewModel = new ConfirmViewModel(message);
            viewModel.DialogResultSet += OnDialogResultSet;
            DataContext = viewModel;
        }

        private void OnDialogResultSet(bool result)
        {
            DialogResultValue = result;
            DialogResult = result; // closes the window
        }
    }
}
