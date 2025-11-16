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
    /// Interaction logic for PositionsToolbarView.xaml
    /// </summary>
    public partial class PositionsToolbarView : AdonisWindow
    {
        public PositionsToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            DataContext = new PositionsToolbarViewModel(eramViewModel);
        }
    }
}
