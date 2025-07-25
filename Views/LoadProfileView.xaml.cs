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
            viewModel.OpenEramWindow += OpenEramWindow;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (DataContext is LoadProfileViewModel vm)
            {
                bool isRenaming = vm.FilteredProfiles.Any(p => p.IsRenaming);
                if (isRenaming) return;
                switch (e.Key)
                {
                    case Key.Enter:
                        if (vm.LastSelectedProfileVM != null && vm.LastSelectedProfileVM.IsSelected)
                        {
                            vm.SelectedProfile = vm.LastSelectedProfileVM.Model;
                            OpenEramWindow();
                            e.Handled = true;
                        }
                        break;
                    case Key.Up:
                        if (vm.FilteredProfiles.Count > 0)
                        {
                            vm.SelectedIndex = Math.Max(0, vm.SelectedIndex - 1);
                            e.Handled = true;
                        }
                        break;
                    case Key.Down:
                        if (vm.FilteredProfiles.Count > 0)
                        {
                            vm.SelectedIndex = Math.Min(vm.FilteredProfiles.Count - 1, vm.SelectedIndex + 1);
                            e.Handled = true;
                        }
                        break;
                }
            }
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

        private void OpenEramWindow()
        {
            if (DataContext is LoadProfileViewModel vm)
            {
                var mainWindow = new EramView(vm.SelectedProfile);
                this.Close();
                mainWindow.ShowDialog();
            }
        }
    }
}
