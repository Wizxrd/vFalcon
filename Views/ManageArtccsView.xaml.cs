using AdonisUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;
using static vFalcon.ViewModels.ManageArtccsViewModel;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for ManageArtccsView.xaml
    /// </summary>
    public partial class ManageArtccsView : AdonisWindow
    {
        ManageArtccsViewModel manageArtccsViewModel = new();
        public ManageArtccsView()
        {
            InitializeComponent();
            DataContext = manageArtccsViewModel;
            this.Closing += OnClosing;
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (manageArtccsViewModel.IsInstallingArtcc)
            {
                string artccId = manageArtccsViewModel.IsInstallingArtccId;
                string path = Loader.LoadFile("ARTCCs", $"{artccId}.json");
                if (manageArtccsViewModel._installCts.TryRemove(artccId, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }
                manageArtccsViewModel.IsInstallingArtccId = string.Empty;
                manageArtccsViewModel.IsInstallingArtcc = false;
                manageArtccsViewModel.InstallingArtccItem.IsInstalling = false;
                manageArtccsViewModel.InstallingArtccItem.InstallUninstallText = "Install";
                manageArtccsViewModel.InstallingArtccItem.StatusText = "Not Installed";
                manageArtccsViewModel.InstallingArtccItem.StatusTextForeground = "#858585";
                manageArtccsViewModel.InstallingArtccItem.ArtccStatusTextForeground = "#858585";
                manageArtccsViewModel.InstallingArtccItem = null;
                if (File.Exists(path)) File.Delete(path);
                Directory.Delete(Loader.LoadFolder($"VideoMaps\\{artccId}"), true);
            }
        }
    }
}
