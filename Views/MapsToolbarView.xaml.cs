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
    /// Interaction logic for MapsToolbarView.xaml
    /// </summary>
    public partial class MapsToolbarView : UserControl
    {
        public MapsToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            DataContext = new MapsToolbarViewModel(eramViewModel);

        }

        public void RebuildFilters()
        {
            if (DataContext is MapsToolbarViewModel vm)
            {
                vm.RebuildFilters();
            }
        }
    }
}
