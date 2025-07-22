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

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for MapsToolbarView.xaml
    /// </summary>
    public partial class MapsToolbarView : UserControl
    {
        public event Action? MapsBackButtonClicked;
        public MapsToolbarView()
        {
            InitializeComponent();
        }

        private void MapsBackButtonClick(object sender, RoutedEventArgs e)
        {
            MapsBackButtonClicked?.Invoke();
        }
    }
}
