using AdonisUI.Controls;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.ViewModels;
using vFalcon.Views;
using static vFalcon.Services.Service.DragService;

namespace vFalcon
{
    /// <summary>
    /// Interaction logic for EramView.xaml
    /// </summary>
    public partial class EramView : AdonisWindow
    {
        // ========================================================
        //                      FIELDS
        // ========================================================
        private readonly EramViewModel eramViewModel;
        private readonly Artcc artcc;
        private readonly Profile profile;

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public EramView(Artcc artcc, Profile profile)
        {
            InitializeComponent();

            this.artcc = artcc;
            this.profile = profile;

            eramViewModel = new EramViewModel(artcc, profile);
            DataContext = eramViewModel;
            eramViewModel.SwitchProfileAction += OpenLoadProfileWindow;
            eramViewModel.GeneralSettingsAction += OpenGeneralSettingsWindow;

            InitializeCursor(eramViewModel);
            LoadWindowSettings();
        }

        // ========================================================
        //                EVENT HANDLERS
        // ========================================================

        private void AltKeyDown(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                if (!eramViewModel.UseAlternateMapLayout)
                {
                    Logger.Debug("ALT", "Detected Alt Key");
                    eramViewModel.UseAlternateMapLayout = true;
                }

                e.Handled = true;
            }
        }

        private void AltKeyUp(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            if (key == Key.LeftAlt || key == Key.RightAlt && eramViewModel.UseAlternateMapLayout)
            {
                eramViewModel.UseAlternateMapLayout = false;
            }
        }

        private void MenuButtonClick(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = !MenuPopup.IsOpen;
        }

        public void OpenLoadProfileWindow()
        {
            LoadProfileView loadProfileView = new LoadProfileView();
            this.Close();
            loadProfileView.ShowDialog();
        }

        public void OpenGeneralSettingsWindow()
        {
            GeneralSettingsView GeneralSettingsView = new GeneralSettingsView(eramViewModel);
            GeneralSettingsView.Owner = this;
            GeneralSettingsView.ShowDialog();
        }

        // ========================================================
        //                PRIVATE METHODS
        // ========================================================

        private void InitializeCursor(EramViewModel eramViewModel)
        {
            UpdateCursorSize((int)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"]);

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
            Left = bounds.Left;
            Top = bounds.Top;
            Width = bounds.Width;
            Height = bounds.Height;

            if ((bool)windowSettings["IsMaximized"])
            {
                WindowState = WindowState.Maximized;
            }
        }
    }
}
