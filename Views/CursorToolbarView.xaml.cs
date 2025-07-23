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
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for CursorToolbarView.xaml
    /// </summary>
    public partial class CursorToolbarView : UserControl
    {
        public event Action? CursorBackButtonClicked;
        public CursorToolbarView()
        {
            InitializeComponent();
        }
        private void CursorBackButtonClick(object sender, RoutedEventArgs e)
        {
            CursorBackButtonClicked?.Invoke();
        }

        private void SizeButtonPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is EramViewModel vm)
            {
                if (e.ChangedButton == MouseButton.Left)
                    vm.DecreaseCursorSizeCommand.Execute(null);
                else if (e.ChangedButton == MouseButton.Middle)
                    vm.IncreaseCursorSizeCommand.Execute(null);
            }
        }
    }
}
