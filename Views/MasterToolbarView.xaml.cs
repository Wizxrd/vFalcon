using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public event Action? ProfileButtonClicked;
        public event Action<ToggleButton>? ActivateButtonClicked;
        public event Action? CursorButtonClicked;
        public event Action? MapsButtonClicked;
        public MasterToolbarView()
        {
            InitializeComponent();
        }

        private void ProfileButtonClick(object sender, RoutedEventArgs e)
        {
            ProfileButtonClicked?.Invoke();
        }

        private void ActivateButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                ActivateButtonClicked?.Invoke(button);
            }
        }

        private void CursorButtonClick(object sender, RoutedEventArgs e)
        {
            CursorButtonClicked?.Invoke();
        }

        private void MapsButtonClick(object sender, RoutedEventArgs e)
        {
            MapsButtonClicked?.Invoke();
        }
    }
}
