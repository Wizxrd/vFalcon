using System.Configuration;
using System.Data;
using System.Windows;
using vFalcon.Helpers;

namespace vFalcon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Logger.Wipe();
        }
    }
}
