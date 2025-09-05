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
using vFalcon.Behaviors;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for MapBrightnessToolbarView.xaml
    /// </summary>
    public partial class MapBrightnessToolbarView : UserControl
    {
        public MapBrightnessToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            DataContext = new MapBrightnessToolbarViewModel(eramViewModel);
        }

        public void RebuildBrightnessBcg()
        {
            if (DataContext is MapBrightnessToolbarViewModel mapBrightnessToolbarViewModel)
            {
                mapBrightnessToolbarViewModel.RebuildBrightnessBcg();
            }
        }

        private void MapBrightButtonPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MapBrightnessToolbarViewModel vm && sender is FrameworkElement fe && fe.DataContext is MapBrightnessButtonViewModel brightness)
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        if (vm.DecreaseBrightnessCommand.CanExecute(brightness))
                            vm.DecreaseBrightnessCommand.Execute(brightness);
                        break;

                    case MouseButton.Middle:
                        if (vm.IncreaseBrightnessCommand.CanExecute(brightness))
                            vm.IncreaseBrightnessCommand.Execute(brightness);
                        break;
                }
            }
        }
    }
}
