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
using vFalcon.Models;
using vFalcon.ViewModels;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for ActivateSectorView.xaml
    /// </summary>
    public partial class ActivateSectorView : AdonisWindow
    {
        public ActivateSectorView(Artcc artcc, Profile profile)
        {
            InitializeComponent();
            ActivateSectorViewModel viewModel = new ActivateSectorViewModel(artcc, profile);
            DataContext = viewModel;
            viewModel.Close += () => this.Close();
        }
    }
}
