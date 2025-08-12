using OpenTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vFalcon.Helpers;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for MenuView.xaml
    /// </summary>
    public partial class MenuView : UserControl
    {
        private MenuViewModel menuViewModel;

        public MenuView()
        {
            InitializeComponent();

            menuViewModel = new MenuViewModel();
            DataContext = menuViewModel;
        }

        public void ActivateSectorButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                if (eramView.DataContext is EramViewModel eramViewModel)
                {
                    if (!eramViewModel.SectorActivated)
                    {
                        eramView.OpenActivateSectorWindow();
                    }
                    else
                    {
                        var dialog = new ConfirmView("Deactivate Sector?")
                        {
                            Title = "Confirm",
                            Owner = eramView
                        };
                        bool? result = dialog.ShowDialog();
                        if (result == true)
                        {
                            eramViewModel.SectorActivated = false;
                            eramViewModel.profile.ActivatedSectorFreq = string.Empty;
                            eramViewModel.profile.ActivatedSectorName = string.Empty;
                            eramViewModel.RadarViewModel.UpdateVatsimDataService();
                            menuViewModel.ActivateSectorText = "Activate";
                        }
                    }
                }
            }
        }

        public void LoadReplayButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                if (eramView.DataContext is EramViewModel eramViewModel)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        InitialDirectory = Loader.LoadFolder("Recordings"), // Set the default folder
                        Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*", // Filter to show only JSON files
                        Title = "Select a JSON File" // Set the title of the dialog
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string selectedFilePath = openFileDialog.FileName;
                        eramViewModel.OnLoadRecording(selectedFilePath);
                    }
                }
            }
        }

        public void StartRecordingButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                if (eramView.DataContext is EramViewModel eramViewModel)
                {
                    eramViewModel.OnToggleRecording();
                    if (eramViewModel.IsRecording)
                    {
                        StartRecordingName.Text = "Stop Recording";
                        StartRecordingKeybind.Text = "Ctrl+Shift+S";
                    }
                    else
                    {
                        StartRecordingName.Text = "Start Recording";
                        StartRecordingKeybind.Text = "Ctrl+Shift+R";
                    }
                }
            }
        }

        public void SwitchProfileButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                eramView.OpenLoadProfileWindow();
            }
        }

        public void GeneralSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                eramView.OpenGeneralSettingsWindow();
            }
        }
    }
}
