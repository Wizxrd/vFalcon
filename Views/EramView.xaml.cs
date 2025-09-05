using AdonisUI.Controls;
using Microsoft.Win32;
using NAudio.Gui;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Renderers;
using vFalcon.ViewModels;
using vFalcon.Views;

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

            eramViewModel = new EramViewModel(this, artcc, profile);
            DataContext = eramViewModel;
            eramViewModel.SwitchProfileAction += OpenLoadProfileWindow;
            eramViewModel.GeneralSettingsAction += OpenGeneralSettingsWindow;
            eramViewModel.ActivateSectorAction += OpenActivateSectorWindow;
            eramViewModel.LoadRecordingAction += OpenLoadRecordingWindow;
            eramViewModel.StartStopRecordingAction += StartStopRecording;
            eramViewModel.ExitRecordingAction += () => MenuPopupControl.ExitRecordingButton.IsEnabled = false;

            InitializeCursor(eramViewModel);
            LoadWindowSettings();
        }

        // ========================================================
        //                EVENT HANDLERS
        // ========================================================

        private bool CtrlKeyDown = false;
        private bool ShiftKeyDown = false;
        private bool JKeyDown = false;

        private void EramKeyDown(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;
            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                if (!eramViewModel.UseAlternateMapLayout)
                {
                    eramViewModel.UseAlternateMapLayout = true;
                    eramViewModel.RadarViewModel.SetZoomOnMouse(true);
                }

                e.Handled = true;
            }
            else if (key == Key.LeftCtrl || key == Key.RightCtrl)
            {
                CtrlKeyDown = true;
            }
            else if (key == Key.J)
            {
                JKeyDown = true;
            }
            else if (key == Key.LeftShift || key == Key.RightShift)
            {
                ShiftKeyDown = true;
            }
        }

        private void EramKeyUp(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            if (key == Key.LeftAlt || key == Key.RightAlt && eramViewModel.UseAlternateMapLayout)
            {
                eramViewModel.UseAlternateMapLayout = false;
                eramViewModel.RadarViewModel.SetZoomOnMouse(false);
            }
            else if (key == Key.LeftCtrl || key == Key.RightCtrl)
            {
                CtrlKeyDown = false;
            }
            else if (key == Key.J)
            {
                JKeyDown = false;
            }
            else if (key == Key.LeftShift || key == Key.RightShift)
            {
                ShiftKeyDown = false;
            }
        }

        private void MenuButtonClick(object sender, RoutedEventArgs e)
        {
            MenuPopup.IsOpen = !MenuPopup.IsOpen;
            if (MenuPopupControl.DataContext is MenuViewModel menuViewModel)
            {
                if (!eramViewModel.SectorActivated)
                {
                    menuViewModel.ActivateSectorText = "Activate";
                }
                else
                {
                    menuViewModel.ActivateSectorText = "Deactivate";
                }
            }
        }

        private void EramMouseDown(object sender, MouseEventArgs e)
        {
            PilotContextViewModel pilotContextViewModel;
            if (PilotContextControl.DataContext is PilotContextViewModel vm)
            {
                pilotContextViewModel = vm;
                pilotContextViewModel.PilotCallsign = string.Empty;
            }
            else return;
            MapState mapState = eramViewModel.RadarViewModel.mapState;
            System.Drawing.Size size = new System.Drawing.Size(mapState.Width, mapState.Height);
            var pos = e.GetPosition((IInputElement)sender);
            SKPoint mousePoint = new SKPoint((float)pos.X, (float)pos.Y);
            Pilot? clickedTarget = eramViewModel.RadarViewModel.EramLeftMouseDown(mousePoint);
            if (clickedTarget == null)
            {
                PilotContext.IsOpen = false;
                return;
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (CtrlKeyDown)
                {
                    if (clickedTarget.LeaderLingLength == 3)
                    {
                        clickedTarget.LeaderLingLength = 0;
                        eramViewModel.RadarViewModel.Redraw();
                        return;
                    }
                    if (clickedTarget.LeaderLingLength < 3)
                    {
                        clickedTarget.LeaderLingLength++;
                        eramViewModel.RadarViewModel.Redraw();
                        return;
                    }
                    PilotContext.IsOpen = false;
                }
                else
                {
                    PilotContext.HorizontalOffset = pos.X;
                    PilotContext.VerticalOffset = pos.Y + 10;
                    PilotContext.PlacementTarget = (FrameworkElement)sender;
                    pilotContextViewModel.Pilot = clickedTarget;
                    pilotContextViewModel.PilotCallsign = clickedTarget.Callsign;
                    pilotContextViewModel.JRingEnabled = clickedTarget.JRingEnabled;
                    pilotContextViewModel.FullDataBlockPosition = clickedTarget.FullDataBlockPosition;
                    pilotContextViewModel.LeaderLineLength = clickedTarget.LeaderLingLength;
                    pilotContextViewModel.RadarViewModel = eramViewModel.RadarViewModel;
                    PilotContext.IsOpen = true;
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (PilotContext.IsOpen) PilotContext.IsOpen = false;
                if (CtrlKeyDown)
                {
                    if (clickedTarget.FullDataBlockPosition == 9)
                    {
                        clickedTarget.FullDataBlockPosition = 1;
                        eramViewModel.RadarViewModel.Redraw();
                        return;
                    }
                    if (clickedTarget.FullDataBlockPosition < 9)
                    {
                        clickedTarget.FullDataBlockPosition++;
                        eramViewModel.RadarViewModel.Redraw();
                        return;
                    }
                }
                else if (ShiftKeyDown)
                {
                    clickedTarget.JRingEnabled = !clickedTarget.JRingEnabled;
                    eramViewModel.RadarViewModel.Redraw();
                }
                else if (JKeyDown)
                {
                    switch (clickedTarget.JRingSize)
                    {
                        case <= 3:
                            clickedTarget.JRingSize = 5;
                            break;
                        case <= 5:
                            clickedTarget.JRingSize = 10;
                            break;
                        case <= 10:
                            clickedTarget.JRingSize = 15;
                            break;
                        case 15:
                            clickedTarget.JRingSize = 20;
                            break;
                        case 20:
                            clickedTarget.JRingSize = 30;
                            break;
                        case 30:
                            clickedTarget.JRingSize = 3;
                            break;
                    }
                    eramViewModel.RadarViewModel.Redraw();
                }
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                if (PilotContext.IsOpen)
                {
                    PilotContext.IsOpen = false;
                }
                eramViewModel.RadarViewModel.EramMiddleMouseDown(sender, e);
            }
        }

        private void EramSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            eramViewModel.RadarViewModel.EramSizeChanged(e);
        }

        private void RemovePilotContext(object? sender, EventArgs e)
        {
            if (PilotContext.IsOpen) PilotContext.IsOpen = false;
        }

        // ========================================================
        //                PUBLIC METHODS
        // ========================================================

        public void StartStopRecording()
        {
            MenuPopupControl.StartRecordingButtonClick(null, null);
        }

        public void OpenLoadRecordingWindow()
        {
            MenuPopupControl.LoadReplayButtonClick(null, null);
        }

        public void OpenLoadProfileWindow()
        {
            LoadProfileView loadProfileView = new LoadProfileView();
            this.Close();
            if (eramViewModel.isRecording)
            {
                eramViewModel.OnToggleRecording();
            }
            loadProfileView.ShowDialog();
        }

        public void OpenGeneralSettingsWindow()
        {
            GeneralSettingsView GeneralSettingsView = new GeneralSettingsView(eramViewModel);
            GeneralSettingsView.Owner = this;
            GeneralSettingsView.ShowDialog();
        }

        public void OpenActivateSectorWindow()
        {
            ActivateSectorView activateSectorView = new ActivateSectorView(eramViewModel.artcc, eramViewModel.profile);
            if (!eramViewModel.SectorActivated)
            {
                activateSectorView.Owner = this;
                if (activateSectorView.DataContext is ActivateSectorViewModel activateSectorViewModel)
                {
                    activateSectorViewModel.SectorActivated += () =>
                    {
                        eramViewModel.SectorActivated = true;
                        eramViewModel.RadarViewModel.UpdateVatsimDataService();
                    };
                }
                activateSectorView.ShowDialog();
            }
            else
            {
                var dialog = new ConfirmView("Deactivate Sector?")
                {
                    Title = "Confirm",
                    Owner = this
                };
                bool? result = dialog.ShowDialog();
                if (result == true && MenuPopupControl.DataContext is MenuViewModel menuViewModel)
                {
                    eramViewModel.SectorActivated = false;
                    eramViewModel.profile.ActivatedSectorFreq = string.Empty;
                    eramViewModel.profile.ActivatedSectorName = string.Empty;
                    eramViewModel.RadarViewModel.UpdateVatsimDataService();
                    menuViewModel.ActivateSectorText = "Activate";
                }
            }
        }

        public void HideCursor()
        {
            if (this.Cursor == Cursors.None) return;
            this.Cursor = Cursors.None;
        }

        public void ShowCursor()
        {
            var uri = new Uri($"pack://application:,,,/Resources/Cursors/Eram{eramViewModel.CursorSize}.cur");
            this.Cursor = new Cursor(Application.GetResourceStream(uri).Stream);
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
