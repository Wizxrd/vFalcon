using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace vFalcon.Resources.Controls
{
    /// <summary>
    /// Interaction logic for NoTearoffButton.xaml
    /// </summary>
    public partial class NoTearoffButton : UserControl
    {
        public NoTearoffButton()
        {
            InitializeComponent();
        }

        private static readonly DependencyProperty NoTearoffLine1TextProperty = DependencyProperty.Register(nameof(NoTearoffLine1Text), typeof(string), typeof(NoTearoffButton), new PropertyMetadata(string.Empty));
        private static readonly DependencyProperty NoTearoffLine2TextProperty = DependencyProperty.Register(nameof(NoTearoffLine2Text), typeof(string), typeof(NoTearoffButton), new PropertyMetadata(string.Empty));
        private static readonly DependencyProperty NoTearoffCommandProperty = DependencyProperty.Register(nameof(NoTearoffCommand), typeof(ICommand), typeof(NoTearoffButton), new PropertyMetadata(null));
        private static readonly DependencyProperty NoTearoffStyleProperty = DependencyProperty.Register(nameof(NoTearoffStyle), typeof(Style), typeof(NoTearoffButton), new PropertyMetadata(null));

        public string NoTearoffLine1Text
        {
            get => (string)GetValue(NoTearoffLine1TextProperty);
            set => SetValue(NoTearoffLine1TextProperty, value);
        }
        public string NoTearoffLine2Text
        {
            get => (string)GetValue(NoTearoffLine2TextProperty);
            set => SetValue(NoTearoffLine2TextProperty, value);
        }
        public ICommand NoTearoffCommand
        {
            get => (ICommand)GetValue(NoTearoffCommandProperty);
            set => SetValue(NoTearoffCommandProperty, value);
        }
        public Style NoTearoffStyle
        {
            get => (Style)GetValue(NoTearoffStyleProperty);
            set => SetValue(NoTearoffStyleProperty, value);
        }
    }
}
