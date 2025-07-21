using AdonisUI.Controls;
using System.Windows.Input;
using vFalcon.Helpers;
using vFalcon.ViewModels;
using vFalcon.Models;

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
            viewModel.Close += () => this.Close();
            viewModel.RequestOpenMainWindow += OpenMainWindow;
            viewModel.RequestNewProfileWindow += OpenNewProfileWindow;
            viewModel.RequestConfirmation += ShowConfirmDialog;
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
                            OpenMainWindow();
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

        private void OpenMainWindow()
        {
            if (DataContext is LoadProfileViewModel vm && vm.SelectedProfile != null)
            {
                var mainWindow = new MainWindowView(vm.SelectedProfile);
                this.Close();
                mainWindow.ShowDialog();
            }
        }
        private Task<NewProfileResult> OpenNewProfileWindow()
        {
            var newProfileView = new NewProfileView
            {
                Owner = this
            };
            newProfileView.ShowDialog();
            if (newProfileView.DataContext is NewProfileViewModel vm)
            {
                return Task.FromResult(new NewProfileResult(vm.WasCreated, vm.NewName));
            }

            return Task.FromResult(new NewProfileResult(false, string.Empty));
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
