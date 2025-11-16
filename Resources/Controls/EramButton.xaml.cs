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
using vFalcon.Commands;
using vFalcon.Helpers;

namespace vFalcon.Resources.Controls
{
    /// <summary>
    /// Interaction logic for TearOffButton.xaml
    /// </summary>
    public partial class EramButton : UserControl
    {
        public EramButton()
        {
            InitializeComponent();
        }

        private static readonly DependencyProperty Line1TextProperty = DependencyProperty.Register(nameof(Line1Text), typeof(string), typeof(EramButton), new PropertyMetadata(string.Empty));

        private static readonly DependencyProperty Line2TextProperty = DependencyProperty.Register(nameof(Line2Text), typeof(string), typeof(EramButton), new PropertyMetadata(string.Empty));

        private static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(nameof(EramButtonStyle), typeof(Style), typeof(EramButton), new PropertyMetadata(null));

        private static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(EramButton), new PropertyMetadata(null));
        private static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(EramButton), new PropertyMetadata(false));

        public string Line1Text
        {
            get => (string)GetValue(Line1TextProperty);
            set => SetValue(Line1TextProperty, value);
        }

        public string Line2Text
        {
            get => (string)GetValue(Line2TextProperty);
            set => SetValue(Line2TextProperty, value);
        }

        public Style EramButtonStyle
        {
            get => (Style)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }
    }
}