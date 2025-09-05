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
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for MasterToolbarView.xaml
    /// </summary>
    public partial class MasterToolbarView : UserControl
    {
        private EramViewModel eramViewModel;
        public MasterToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            this.eramViewModel = eramViewModel;
            DataContext = new MasterToolbarViewModel(eramViewModel);
        }

        public void DecreaseVelocityVector()
        {
            {
                if (eramViewModel.VelocityVector > 0)
                {
                    eramViewModel.VelocityVector /= 2;
                    eramViewModel.RadarViewModel.Redraw();
                }
            }
        }

        public void IncreaseVelocityVector()
        {
            if (eramViewModel.VelocityVector == 0)
            {
                eramViewModel.VelocityVector = 1;
                eramViewModel.RadarViewModel.Redraw();
                return;
            }
            if (eramViewModel.VelocityVector < 8)
            {
                eramViewModel.VelocityVector *= 2;
                eramViewModel.RadarViewModel.Redraw();
            }
        }

        private void VelocityVectorMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DecreaseVelocityVector();
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                IncreaseVelocityVector();
            }
        }
    }
}
