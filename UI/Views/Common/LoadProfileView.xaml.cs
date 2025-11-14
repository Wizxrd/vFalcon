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
            loadProfileViewModel.OpenManageArtccsView += OpenManagedArtccsView;
            loadProfileViewModel.OpenNewProfileView += OpenNewProfileView;
            loadProfileViewModel.OpenMainWindowView += OpenMainWindowView;
        }

        private void OpenManagedArtccsView()
        {
            ManageArtccsView manageArtccsView = new ManageArtccsView();
            manageArtccsView.Owner = this;
            manageArtccsView.ShowDialog();
        }

        private void OpenNewProfileView()
        {
            NewProfileView newProfileView = new NewProfileView();
            newProfileView.Owner = this;
            newProfileView.ShowDialog();
        }

        private void OpenMainWindowView()
        {
            App.Profile = loadProfileViewModel.SelectedProfile;
            App.Artcc = loadProfileViewModel.SelectedProfileArtcc;
            MainWindowView mainWindowView = new MainWindowView();
            Application.Current.MainWindow = mainWindowView;
            this.Close();
            Logger.LogLevelThreshold = (LogLevel)App.Profile.LogLevel;
            mainWindowView.ShowDialog();
        }
    }
}
