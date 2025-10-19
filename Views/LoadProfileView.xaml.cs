using AdonisUI.Controls;
using System.Windows.Input;
using vFalcon.Helpers;
using vFalcon.Services.Service;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for LoadProfileView.xaml
    /// </summary>
    public partial class LoadProfileView : AdonisWindow
    {
        private LoadProfileViewModel loadProfileViewModel = new();

        public LoadProfileView()
        {
            InitializeComponent();
            DataContext = loadProfileViewModel;
            loadProfileViewModel.RequestConfirmation += ShowConfirmDialog;
            loadProfileViewModel.OpenEramWindow += OpenEramWindow;
            loadProfileViewModel.OpenNewProfileView += OpenNewProfileView;
            loadProfileViewModel.OpenManageArtccsView += OpenManagedArtccsView;
        }

        private void OpenManagedArtccsView()
        {
            ManageArtccsView manageArtccsView = new ManageArtccsView();
            manageArtccsView.Owner = this;
            manageArtccsView.ShowDialog();
            loadProfileViewModel.LoadProfiles();
        }

        private void OpenNewProfileView()
        {
            NewProfileView newProfileView = new();
            newProfileView.Owner = this;
            newProfileView.ShowDialog();
            loadProfileViewModel.LoadProfiles();
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
                            vm.SelectedProfile.LastUsedAt = DateTime.UtcNow;
                            loadProfileViewModel.profileService.Save(vm.SelectedProfile);
                            OpenEramWindow();
                            e.Handled = true;
                        }
                        break;
                    case Key.Up:
                        if (vm.FilteredProfiles.Count> 0)
                        {
                            vm.SelectedIndex = Math.Max(0, vm.SelectedIndex - 1);
                            e.Handled = true;
                        }
                        break;
                    case Key.Down:
                        if (vm.FilteredProfiles.Count> 0)
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
                try
                {
                    var mainWindow = new EramView(vm.SelectedProfileArtcc, vm.SelectedProfile);
                    this.Close();
                    mainWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    Logger.Error("LoadProfileView.OpenEramWindow", ex.ToString());
                }
            }
        }
    }
}
