using AdonisUI.Controls;
using Newtonsoft.Json;
using System.Windows;
using vFalcon.Models;
using vFalcon.UI.ViewModels.Common;
using vFalcon.Utils;
namespace vFalcon.UI.Views.Common
{
    public partial class LoadProfileView : AdonisWindow
    {
        private LoadProfileViewModel loadProfileViewModel = new();
        public LoadProfileView()
        {
            InitializeComponent();
            DataContext = loadProfileViewModel;
            loadProfileViewModel.OpenManageArtccsView += App.ViewManager.OpenManagedArtccsView;
            loadProfileViewModel.OpenNewProfileView += App.ViewManager.OpenNewProfileView;
            loadProfileViewModel.OpenMainWindowView += OpenMainWindowView;
        }

        private void OpenMainWindowView()
        {
            try
            {
                App.Profile = loadProfileViewModel.SelectedProfile;
                App.Artcc = loadProfileViewModel.SelectedProfileArtcc;
                MainWindowView mainWindowView = new MainWindowView();
                Application.Current.MainWindow = mainWindowView;
                this.Close();
                Logger.LogLevelThreshold = (LogLevel)App.Profile.GeneralSettings.LogLevel;
                mainWindowView.ShowDialog();
            }
            catch (Exception ex)
            {
                Logger.Error("LoadProfileView.OpenMainWindowView", ex.ToString());
            }
        }
    }
}
