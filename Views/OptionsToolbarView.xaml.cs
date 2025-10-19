using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for OptionsToolbarView.xaml
    /// </summary>
    public partial class OptionsToolbarView : UserControl
    {
        public MapsOptionsToolbarView MapsOptionsToolbarView;
        public OptionsToolbarViewModel optionsToolbarViewModel;
        private EramViewModel eramViewModel;
        public OptionsToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            this.eramViewModel = eramViewModel;
            optionsToolbarViewModel = new OptionsToolbarViewModel(eramViewModel);
            MapsOptionsToolbarView = new MapsOptionsToolbarView(eramViewModel);
            DataContext = optionsToolbarViewModel;
        }

        public void ToggleRecordingTexts()
        {
            eramViewModel.OnToggleRecording();
            if (eramViewModel.IsRecording)
            {
                optionsToolbarViewModel.RecordingText = "Stop Recording";
                optionsToolbarViewModel.RecordingInput = "Alt+R";
            }
            else
            {
                optionsToolbarViewModel.RecordingText = "Start Recording";
                optionsToolbarViewModel.RecordingInput = "Alt+R";
            }
        }

        private void OnRecordingButtonClick(object sender, RoutedEventArgs e)
        {
            //RecordingPopup.IsOpen = !RecordingPopup.IsOpen;
        }

        private void OnReplaysButtonClick(object sender, RoutedEventArgs e)
        {
            //ReplaysPopup.IsOpen = !ReplaysPopup.IsOpen;
        }

        private void OnMapsButtonClick(object sender, RoutedEventArgs e)
        {
            MapsOptionsToolbarView.Owner = eramViewModel.eramView;
            MapsOptionsToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            MapsOptionsToolbarView.Closed += (_, __) => MapsOptionsToolbarView = null;
            MapsOptionsToolbarView.Show();
        }

        private void OnSettingsButtonClick(object sender, EventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                eramView.OpenGeneralSettingsWindow();
            }
        }

        private void OnToolsButtonClick(object sender, RoutedEventArgs e)
        {
            //ToolsPopup.IsOpen = !ToolsPopup.IsOpen;
        }

        private void OnProfilesButtonClick(object sender, EventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                eramView.OpenLoadProfileWindow();
            }
        }

        private void OnDiscordButtonClick(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://discord.gg/8g7MFs5QfB",
                UseShellExecute = true
            });
        }

        private void OnGithubButtonClick(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/Wizxrd/vFalcon",
                UseShellExecute = true
            });
        }
    }
}
