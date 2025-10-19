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
    /// Interaction logic for FindOptionsToolbarView.xaml
    /// </summary>
    public partial class FindOptionsToolbarView : AdonisWindow
    {
        public FindOptionsToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            DataContext = new FindOptionsToolbarViewModel(eramViewModel);
        }
    }
}
