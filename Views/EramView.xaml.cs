using AdonisUI.Controls;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;
using vFalcon.Views;

namespace vFalcon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EramView : AdonisWindow
    {
        // Fields
        private EramViewModel eramViewModel;
        private Artcc artcc;
        private Profile profile;

        // Constructor
        public EramView(Artcc artcc, Profile profile)
        {
            InitializeComponent();
            this.artcc = artcc;
            this.profile = profile;
            eramViewModel = new EramViewModel(artcc, profile);
            DataContext = eramViewModel;

            InitializeCursor(eramViewModel);
            LoadWindowSettings();
        }

        // Methods
        private void InitializeCursor(EramViewModel eramViewModel)
        {
            UpdateCursorSize((int)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"]); // initial call to set the cursor size!
            eramViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(eramViewModel.CursorSize))
                {
                    UpdateCursorSize(eramViewModel.CursorSize);
                }
            };
        }

        private void UpdateCursorSize(int cursorSize)
        {
            var uri = new Uri($"pack://application:,,,/Resources/Cursors/Eram{cursorSize}.cur");
            this.Cursor = new Cursor(Application.GetResourceStream(uri).Stream);
        }

        private void LoadWindowSettings()
        {
            JObject windowSettings = (JObject)profile.DisplayWindowSettings[0]["WindowSettings"];
            string boundsString = (string)windowSettings["Bounds"];
            double[] parts = boundsString.Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();

            Rect bounds = new Rect(parts[0], parts[1], parts[2], parts[3]);
            this.Left = bounds.Left;
            this.Top = bounds.Top;
            this.Width = bounds.Width;
            this.Height = bounds.Height;

            if ((bool)windowSettings["IsMaximized"])
            {
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}
