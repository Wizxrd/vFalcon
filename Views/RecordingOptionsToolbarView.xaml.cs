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
    /// Interaction logic for RecordingOptionsToolbarView.xaml
    /// </summary>
    public partial class RecordingOptionsToolbarView : UserControl
    {
        public RecordingOptionsToolbarView()
        {
            InitializeComponent();
        }

        public void StartRecordingButtonClick(object? sender, RoutedEventArgs? e)
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

    }
}
