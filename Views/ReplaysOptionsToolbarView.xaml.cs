using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using vFalcon.Helpers;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for ReplaysOptionsToolbarView.xaml
    /// </summary>
    public partial class ReplaysOptionsToolbarView : UserControl
    {
        public ReplaysOptionsToolbarView()
        {
            InitializeComponent();
        }
        public void ExitRecordingButtonClick(object? sewnder, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                if (eramView.DataContext is EramViewModel eramViewModel)
                {
                    eramViewModel.ExitRecording();
                    ExitRecordingButton.IsEnabled = false;
                }
            }
        }

        public void LoadReplayButtonClick(object? sender, RoutedEventArgs? e)
        {
            var window = Window.GetWindow(this);
            if (window is EramView eramView)
            {
                if (eramView.DataContext is EramViewModel eramViewModel)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        InitialDirectory = Loader.LoadFolder("Recordings"),
                        Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                        Title = "Select a JSON File"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string selectedFilePath = openFileDialog.FileName;
                        eramViewModel.OnLoadRecording(selectedFilePath);
                        ExitRecordingButton.IsEnabled = true;
                    }
                }
            }
        }
    }
}
