using System.Windows;
using AdonisUI.Controls;
using vFalcon.Nasr;
using vFalcon.Updaters;
using vFalcon.Utils;
namespace vFalcon.UI.Views.Common;

public partial class LoadingView : AdonisWindow
{
    private NavData navData { get; set; } = new(null);
    public LoadingView()
    {
        InitializeComponent();
        Start();
    }

    private async void Start()
    {
        Logger.Info("LoadingView", "Starting");
        await Github.CheckForUpdate(TextBlockLoading);
        await new NavData(null).Run();
        await vNas.CheckForUpdates();
        LoadProfileView loadProfileView = new LoadProfileView();
        Application.Current.MainWindow = loadProfileView;
        this.Close();
        loadProfileView.ShowDialog();
    }
}
