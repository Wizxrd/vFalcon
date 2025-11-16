using AdonisUI.Controls;
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
using System.Windows.Shapes;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for AircraftListToolbarView.xaml
    /// </summary>
    public partial class AircraftListToolbarView : AdonisWindow
    {
        public AircraftListToolbarViewModel aircraftListToolbarViewModel;
        public AircraftListToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            aircraftListToolbarViewModel = new AircraftListToolbarViewModel(eramViewModel);
            DataContext = aircraftListToolbarViewModel;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            aircraftListToolbarViewModel.StopRefreshTimer();
        }
    }
}
