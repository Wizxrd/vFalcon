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
    /// Interaction logic for RadarView.xaml
    /// </summary>
    public partial class RadarView : UserControl
    {
        public RadarView()
        {
            InitializeComponent();
            Loaded += RadarViewLoaded;
        }

        private void RadarViewLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is RadarViewModel radarVM)
            {
                radarVM.InvalidateCanvas = () =>
                Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Render,
                    new Action(() => RadarCanvas.InvalidateVisual())
                );
            }
        }
    }
}
