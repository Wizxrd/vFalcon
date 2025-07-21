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
using vFalcon.Models;
using AdonisUI.Controls;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for SaveProfileAsView.xaml
    /// </summary>
    public partial class SaveProfileAsView : AdonisWindow
    {
        public SaveProfileAsView(Profile profile)
        {
            InitializeComponent();
            var viewModel = new SaveProfileAsViewModel(profile);
            DataContext = viewModel;
            NameTextBox.Focus();
            NameTextBox.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                NameTextBox.SelectAll();
            }));
            viewModel.ProfileName = profile.Name;
            viewModel.Close += () => this.Close();
        }
    }
}
