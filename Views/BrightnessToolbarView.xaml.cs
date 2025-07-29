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
    /// Interaction logic for BrightnessToolbarView.xaml
    /// </summary>
    public partial class BrightnessToolbarView : UserControl
    {
        public BrightnessToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            DataContext = new BrightnessToolbarViewModel(eramViewModel);
        }

        private void BackgroundButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BrightnessToolbarViewModel brightnessViewModel)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (brightnessViewModel.BackgroundValue> 0)
                    {
                        brightnessViewModel.BackgroundValue -= 2;
                        return;
                    }
                }
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    if (brightnessViewModel.BackgroundValue < 60)
                    {
                        brightnessViewModel.BackgroundValue +=2;
                        return;
                    }
                }
            }
        }

        private void BacklightButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BrightnessToolbarViewModel brightnessViewModel)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (brightnessViewModel.BrightnessValue> 0)
                    {
                        brightnessViewModel.BrightnessValue -= 2;
                        return;
                    }
                }
                if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    if (brightnessViewModel.BrightnessValue < 100)
                    {
                        brightnessViewModel.BrightnessValue += 2;
                        return;
                    }
                }
            }
        }
    }
}
