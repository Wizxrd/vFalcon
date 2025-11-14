using AdonisUI.Controls;
using System.ComponentModel;
using System.IO;
using vFalcon.UI.ViewModels.Common;
using vFalcon.Utils;
namespace vFalcon.UI.Views.Common;

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
            string path = PathFinder.GetFilePath("ARTCCs", $"{artccId}.json");
            if (manageArtccsViewModel.CancellationTokenSources.TryRemove(artccId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
            manageArtccsViewModel.IsInstallingArtccId = string.Empty;
            manageArtccsViewModel.IsInstallingArtcc = false;
            manageArtccsViewModel.InstalledArtcc.IsInstalling = false;
            manageArtccsViewModel.InstalledArtcc.InstallUninstallText = "Install";
            manageArtccsViewModel.InstalledArtcc.StatusText = "Not Installed";
            manageArtccsViewModel.InstalledArtcc.StatusTextForeground = "#858585";
            manageArtccsViewModel.InstalledArtcc.ArtccStatusTextForeground = "#858585";
            manageArtccsViewModel.InstalledArtcc = null;
            if (File.Exists(path)) File.Delete(path);
            Directory.Delete(PathFinder.GetFolderPath($"VideoMaps\\{artccId}"), true);
        }
    }
}
