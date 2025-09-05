using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vFalcon.Helpers;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    public partial class ReplayControlsView : UserControl
    {
        private EramViewModel eramViewModel;
        private bool mouseDown = false;
        private bool paused = false;
        public ReplayControlsView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            this.eramViewModel = eramViewModel;
            DataContext = new ReplayControlsViewModel(eramViewModel);
            PlaybackSpeedComboBox.SelectedIndex = 0;
        }

        private void OnPlaybackSpeedSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (eramViewModel == null || eramViewModel.playbackService == null) return;
            if (PlaybackSpeedComboBox.SelectedItem is ComboBoxItem item)
            {
                string content = item.Content.ToString() ?? "1x";
                string numberOnly = content.Replace("x", "");
                if (int.TryParse(numberOnly, out int speed))
                {
                    eramViewModel.playbackService.SetPlaybackSpeed(speed);
                }
            }
        }

        public void RewindButtonClick(object? sender, RoutedEventArgs? e)
        {
            paused = true;
            eramViewModel.playbackService.Pause();
            PlayPauseButtonImage.Source = new BitmapImage(new Uri(Loader.LoadFile("Resources/Images", "Play.png")));
            if (eramViewModel.playbackService.Tick > 0) eramViewModel.playbackService.Tick--;
            eramViewModel.playbackService.PlaybackTimerTick(null, null);
        }

        public void FastForwardButtonClick(object? sender, RoutedEventArgs? e)
        {
            paused = true;
            eramViewModel.playbackService.Pause();
            PlayPauseButtonImage.Source = new BitmapImage(new Uri(Loader.LoadFile("Resources/Images", "Play.png")));
            if (eramViewModel.playbackService.Tick < eramViewModel.playbackService.GetTotalTickCount())
            {
                eramViewModel.playbackService.Tick++;
                eramViewModel.playbackService.PlaybackTimerTick(null, null);
            }
        }

        public void PlayPauseButtonClick(object? sender, RoutedEventArgs? e)
        {
            if (paused)
            {
                paused = false;
                PlayPauseButtonImage.Source = new BitmapImage(new Uri(Loader.LoadFile("Resources/Images", "Pause.png")));
                eramViewModel.playbackService.Play();
            }
            else
            {
                paused = true;
                PlayPauseButtonImage.Source = new BitmapImage(new Uri(Loader.LoadFile("Resources/Images", "Play.png")));
                eramViewModel.playbackService.Pause();
            }
        }

        private void OnPreviewMouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            paused = true;
            eramViewModel.playbackService.Pause();
            PlayPauseButtonImage.Source = new BitmapImage(new Uri(Loader.LoadFile("Resources/Images", "Play.png")));
        }

        public void OnPreviewMouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        public void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                eramViewModel.playbackService.PlaybackTimerTick(null, null);
            }
        }
    }
}
