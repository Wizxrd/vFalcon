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
using vFalcon.Helpers;
using vFalcon.ViewModels;
using MessageBox = vFalcon.Services.Service.MessageBoxService;

namespace vFalcon.Views
{
    /// <summary>
    /// Interaction logic for GeneralSettingsView.xaml
    /// </summary>
    public partial class GeneralSettingsView : AdonisWindow
    {
        private EramViewModel eramViewModel;
        public GeneralSettingsViewModel generalSettingsViewModel;
        public GeneralSettingsView(EramViewModel eramViewModel)
        {
            InitializeComponent();
            this.eramViewModel = eramViewModel;
            generalSettingsViewModel = new GeneralSettingsViewModel(eramViewModel);
            DataContext = generalSettingsViewModel;
            generalSettingsViewModel.Close += () => this.Close();
        }

        public bool PttKeyDown = false;

        public void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (generalSettingsViewModel.IsScanningForPtt)
            {
                Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;
                int keyId = KeyInterop.VirtualKeyFromKey(key);
                string keyName = key.ToString();
                bool confirmed = MessageBox.Confirm($"Do you wish to use {keyName} as PTT?");
                if (confirmed)
                {
                    eramViewModel.profile.PttKey = keyId;
                    generalSettingsViewModel.PttButton = keyName;
                    generalSettingsViewModel.PttFound = true;
                    generalSettingsViewModel.OnCancelPttScanCommand();
                }
                else
                {
                    generalSettingsViewModel.PttFound = false;
                    generalSettingsViewModel.OnCancelPttScanCommand();
                }
            }
            else if (eramViewModel.profile.PttKey.HasValue)
            {
                Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;
                int keyId = KeyInterop.VirtualKeyFromKey(key);
                if (keyId == eramViewModel.profile.PttKey)
                {
                    PttKeyDown = true;
                }
            }
        }
    }
}
