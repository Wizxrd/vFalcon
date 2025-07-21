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
    /// Interaction logic for ProfileToolbarView.xaml
    /// </summary>
    public partial class ProfileToolbarView : UserControl
    {
        public event Action? ProfileBackButtonClicked;
        public ProfileToolbarView()
        {
            InitializeComponent();
        }

        private void ProfileBackButtonClick(object sender, RoutedEventArgs e)
        {
            ProfileBackButtonClicked?.Invoke();
        }
    }
}
