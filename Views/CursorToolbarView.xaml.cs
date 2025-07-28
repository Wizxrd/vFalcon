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
using vFalcon.Helpers;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for CursorToolbarView.xaml
    /// </summary>
    public partial class CursorToolbarView : UserControl
    {
        public CursorToolbarView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            DataContext = new CursorToolbarViewModel(eramViewModel);
        }

        private async void CursorSizeButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CursorToolbarViewModel cursorViewModel)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (cursorViewModel.CursorSize > 1)
                    {
                        --cursorViewModel.CursorSize;
                        return;
                    }
                    await Sound.Play(Loader.LoadFile("Sounds", "Error.wav"));
                }
                else if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    if (cursorViewModel.CursorSize < 5)
                    {
                        ++cursorViewModel.CursorSize;
                        return;
                    }
                    await Sound.Play(Loader.LoadFile("Sounds", "Error.wav"));
                }
            }
        }
    }
}
