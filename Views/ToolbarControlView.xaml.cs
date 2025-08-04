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
using vFalcon.Resources.Controls;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for ToolbarView.xaml
    /// </summary>
    public partial class ToolbarControlView : UserControl
    {

        private EramViewModel _eramViewModel;

        public ToolbarControlView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            _eramViewModel = eramViewModel;
            
            DataContext = new ToolbarControlViewModel(eramViewModel);
            Loaded += ToolbarControlView_Loaded;
        }

        private void ToolbarControlView_Loaded(object sender, RoutedEventArgs e)
        {
            MenuButton.Dispatcher.BeginInvoke(new Action(() =>
            {
                var width = MenuButton.ActualWidth;
                var height = MenuButton.ActualHeight;
                _eramViewModel.OnMenuButtonMeasured( width, height );
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }
}
