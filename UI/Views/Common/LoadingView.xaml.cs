using System.Windows;
using AdonisUI.Controls;
using vFalcon.Nasr;
using vFalcon.Updaters;
using vFalcon.Utils;
namespace vFalcon.UI.Views.Common;

public partial class LoadingView : AdonisWindow
{
    public LoadingView()
    {
        InitializeComponent();
        Start();
    }

    private async void Start()
    {
        await Github.CheckForUpdate(TextBlockLoading);
        NavData nasr = new NavData(null)
        {
            ForceDownload = false,
            SwapNavDate = false,
            TextBlockLoading = TextBlockLoading,
            Delay = 250,
        };
        await nasr.Run();
        TextBlockLoading.Text = "Nav data up-to-date";
        await vNas.CheckForUpdates(TextBlockLoading);
        LoadProfileView loadProfileView = new LoadProfileView();
        Application.Current.MainWindow = loadProfileView;
        this.Close();
        loadProfileView.ShowDialog();
    }
}
