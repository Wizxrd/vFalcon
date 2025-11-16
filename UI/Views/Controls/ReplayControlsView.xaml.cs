using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using vFalcon.UI.ViewModels.Controls;
using vFalcon.Utils;
namespace vFalcon.UI.Views.Controls;

public partial class ReplayControlsView : UserControl
{
    private bool mouseDown = false;
    private bool paused = false;
    public ReplayControlsView()
    {
        InitializeComponent();
        DataContext = new ReplayControlsViewModel();
        PlaybackSpeedComboBox.SelectedIndex = 0;
    }

    private void OnPlaybackSpeedSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (App.MainWindowViewModel == null || App.MainWindowViewModel.PlaybackService == null) return;
        if (PlaybackSpeedComboBox.SelectedItem is ComboBoxItem item)
        {
            string content = item.Content.ToString() ?? "1x";
            string numberOnly = content.Replace("x", "");
            if (int.TryParse(numberOnly, out int speed))
            {
                App.MainWindowViewModel.PlaybackService.SetPlaybackSpeed(speed);
            }
        }
    }

    public void RewindButtonClick(object? sender, RoutedEventArgs? e)
    {
        App.MainWindowViewModel.OnRewindCommand();
    }

    public void FastForwardButtonClick(object? sender, RoutedEventArgs? e)
    {
        App.MainWindowViewModel.OnFastForwardCommand();
    }

    public void PlayPauseButtonClick(object? sender, RoutedEventArgs? e)
    {
        App.MainWindowViewModel.OnPlayPauseCommand();
    }

    private void OnPreviewMouseDown(object sender, MouseEventArgs e)
    {
        mouseDown = true;
        paused = true;
        App.MainWindowViewModel.PlaybackService.Pause();
        PlayPauseButtonImage.Source = new BitmapImage(new Uri(PathFinder.GetFilePath("Resources/Images", "Play.png")));
    }

    public void OnPreviewMouseUp(object sender, MouseEventArgs e)
    {
        mouseDown = false;
    }

    public void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (mouseDown)
        {
            App.MainWindowViewModel.PlaybackService.Refresh();
        }
    }
}
