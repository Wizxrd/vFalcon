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
    /// Interaction logic for ControllersList.xaml
    /// </summary>
    public partial class ControllersListView : AdonisWindow
    {
        public ControllersListViewModel controllersListViewModel;
        public ControllersListView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            controllersListViewModel = new ControllersListViewModel(eramViewModel);
            DataContext = controllersListViewModel;
        }
    }
}
