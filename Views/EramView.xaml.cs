using AdonisUI.Controls;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using vFalcon.Helpers;
using vFalcon.Models;
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
        public GeneralSettingsView generalSettingsView;
        public bool PttKeyDown = false;

        // ========================================================
        //                  CONSTRUCTOR
        // ========================================================
        public EramView(Artcc artcc, Profile profile)
        {
            
            InitializeComponent();
            Application.Current.MainWindow = this;
            this.artcc = artcc;
            this.profile = profile;

            eramViewModel = new EramViewModel(this, artcc, profile);
            DataContext = eramViewModel;
            eramViewModel.SwitchProfileAction += OpenLoadProfileWindow;
            eramViewModel.GeneralSettingsAction += OpenGeneralSettingsWindow;
            eramViewModel.ActivateSectorAction += OpenActivateSectorWindow;
            eramViewModel.LoadRecordingAction += OpenLoadRecordingWindow;
            eramViewModel.StartStopRecordingAction += StartStopRecording;
            //eramViewModel.ExitRecordingAction += () => MenuPopupControl.ExitRecordingButton.IsEnabled = false;

            this.LostFocus += (_,__) => OnLostFocus();
            this.Deactivated += (_, __) => OnLostFocus();
            this.Activated += (_, __) => OnLostFocus();

            InitializeCursor(eramViewModel);
            LoadWindowSettings();
        }
        private void OnLostFocus()
        {
            CtrlKeyDown = false;
            ShiftKeyDown = false;
            JKeyDown = false;
            AltKeyDown = false;
            QKeyDown = false;
        }

        // ========================================================
        //                EVENT HANDLERS
        // ========================================================

        private bool CtrlKeyDown = false;
        private bool ShiftKeyDown = false;
        private bool JKeyDown = false;
        private bool AltKeyDown = false;
        private bool QKeyDown = false;

        private void EramMouseMove(object sender, MouseEventArgs e)
        {
            if (eramViewModel.ShowLatLon)
            {
                var pos = e.GetPosition((IInputElement)sender);
                SKPoint mousePoint = new SKPoint((float)pos.X, (float)pos.Y);
                eramViewModel.EramMouseMove(mousePoint);
            }
        }

        private void EramKeyDown(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;
            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                AltKeyDown = true;
                eramViewModel.RadarViewModel.mapState.ZoomOnMouse = true;
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
            else if (key == Key.Q)
            {
                QKeyDown = true;
            }
            if (eramViewModel.profile.PttKey is int vk && vk > 0)
            {
                Key pttKey = KeyInterop.KeyFromVirtualKey(vk);
                if (pttKey == key)
                {
                    eramViewModel.MicRecorder.SetPtt(true);
                }
            }
        }

        private void EramKeyUp(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                AltKeyDown = false;
                eramViewModel.RadarViewModel.mapState.ZoomOnMouse = false;
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
            else if (key == Key.Q)
            {
                QKeyDown = false;
            }
            if (eramViewModel.profile.PttKey is int vk && vk > 0)
            {
                Key pttKey = KeyInterop.KeyFromVirtualKey(vk);
                if (pttKey == key)
                {
                    eramViewModel.MicRecorder.SetPtt(false);
                }
            }
        }

        private void MenuButtonClick(object sender, RoutedEventArgs e)
        {
            //MenuPopup.IsOpen = !MenuPopup.IsOpen;
            //if (MenuPopupControl.DataContext is MenuViewModel menuViewModel)
            //{
            //    if (!eramViewModel.SectorActivated)
            //    {
            //        menuViewModel.ActivateSectorText = "Activate";
            //    }
            //    else
            //    {
            //        menuViewModel.ActivateSectorText = "Deactivate";
            //    }
            //}
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
                    string flightRules = (string)clickedTarget.FlightPlan?["flight_rules"] ?? string.Empty;
                    string flightPlan = $"{clickedTarget.FlightPlan?["departure"]} {clickedTarget.FlightPlan?["route"]} {clickedTarget.FlightPlan?["arrival"]}";
                    flightPlan = flightPlan.Replace("DCT ", "");
                    if (flightRules == "I") flightRules = "IFR";
                    else if (flightRules == "V") flightRules = "VFR";
                    string requestedAltitude = (string)clickedTarget.FlightPlan?["altitude"];
                    if (requestedAltitude.Contains("VFR")) flightRules = "VFR";
                    PilotContext.HorizontalOffset = pos.X;
                    PilotContext.VerticalOffset = pos.Y + 10;
                    PilotContext.PlacementTarget = (FrameworkElement)sender;
                    pilotContextViewModel.Pilot = clickedTarget;
                    pilotContextViewModel.Time = $"Time: {DateTime.Now:MM/dd/yyyy HH:mm:ss}";
                    pilotContextViewModel.PilotCallsign = $"Callsign: {clickedTarget.Callsign}";
                    pilotContextViewModel.CID = $"CID: {clickedTarget.CID}";
                    pilotContextViewModel.Type = $"Type: {clickedTarget.FlightPlan?["aircraft_faa"]}";
                    pilotContextViewModel.ReportedAltitude = $"Reported Altitude: {clickedTarget.Altitude}";
                    pilotContextViewModel.RequestedAltitude = $"Requested Altitude: {requestedAltitude}";
                    pilotContextViewModel.Heading = $"Course Mag. Heading: {clickedTarget.Heading}";
                    pilotContextViewModel.Speed = $"Speed: {clickedTarget.GroundSpeed}";
                    pilotContextViewModel.AssignedBeacon = $"Assigned Beacon: {clickedTarget.FlightPlan?["assigned_transponder"]}";
                    pilotContextViewModel.FlightRules = $"Flight Rules: {flightRules}";
                    pilotContextViewModel.LatLon = $"Lat/Lon: {clickedTarget.Latitude}/{clickedTarget.Longitude}";
                    pilotContextViewModel.JRingSize = clickedTarget.JRingSize;
                    pilotContextViewModel.JRingEnabled = clickedTarget.JRingEnabled;
                    pilotContextViewModel.FullDataBlockPosition = clickedTarget.FullDataBlockPosition;
                    pilotContextViewModel.LeaderLineLength = clickedTarget.LeaderLingLength;
                    pilotContextViewModel.RadarViewModel = eramViewModel.RadarViewModel;
                    pilotContextViewModel.FlightPlan = flightPlan;
                    pilotContextViewModel.FullRouteVisibility = Visibility.Collapsed;
                    pilotContextViewModel.DatablockType = clickedTarget.DatablockType == "ERAM" ? 0 : clickedTarget.DatablockType == "STARS" ? 1 : -1;
                    if (eramViewModel.isPlaybackMode)
                    {
                        pilotContextViewModel.FullRouteVisibility = Visibility.Visible;
                    }
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
                else if (AltKeyDown)
                {
                    clickedTarget.DwellLock = !clickedTarget.DwellLock;
                    eramViewModel.RadarViewModel.Redraw();
                }
                else if (JKeyDown)
                {
                    if (!clickedTarget.JRingEnabled) return;
                    switch (clickedTarget.JRingSize)
                    {
                        case <= 1:
                            clickedTarget.JRingSize = 3;
                            break;
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
                            clickedTarget.JRingSize = 25;
                            break;
                        case 25:
                            clickedTarget.JRingSize = 30;
                            break;
                        case 30:
                            clickedTarget.JRingSize = 1;
                            break;
                    }
                    eramViewModel.RadarViewModel.Redraw();
                }
                else if (QKeyDown)
                {
                    pilotContextViewModel.Pilot = clickedTarget;
                    pilotContextViewModel.OnDisplayRouteCommand();
                    eramViewModel.RadarViewModel.Redraw();
                }
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                if (PilotContext.IsOpen)
                {
                    PilotContext.IsOpen = false;
                }
                else eramViewModel.RadarViewModel.EramMiddleMouseDown(sender, e);
            }
        }

        private void EramLocationChanged(object? sender, EventArgs e)
        {
            eramViewModel.profile.WindowSettings["Bounds"] = $"{this.Left},{this.Top},{this.Width},{this.Height}";
        }

        private void EramSizeChanged(object? sender, SizeChangedEventArgs e)
        {
            eramViewModel.RadarViewModel.EramSizeChanged(e);
            eramViewModel.profile.WindowSettings["Bounds"] = $"{this.Left},{this.Top},{this.Width},{this.Height}";
        }

        private void RemovePilotContext(object? sender, EventArgs e)
        {
            if (PilotContext.IsOpen) PilotContext.IsOpen = false;
            eramViewModel.profile.WindowSettings["IsMaximized"] = (WindowState == WindowState.Maximized);
        }

        // ========================================================
        //                PUBLIC METHODS
        // ========================================================

        public void StartStopRecording()
        {
            //MenuPopupControl.StartRecordingButtonClick(null, null);
        }

        public void OpenLoadRecordingWindow()
        {
            //MenuPopupControl.LoadReplayButtonClick(null, null);
        }

        public void OpenLoadProfileWindow()
        {
            LoadProfileView loadProfileView = new LoadProfileView();
            Application.Current.MainWindow = loadProfileView;
            this.Close();
            if (eramViewModel.isRecording)
            {
                eramViewModel.OnToggleRecording();
            }
            eramViewModel.RadarViewModel.StopDatablockCycle();
            eramViewModel.OptionsToolbarContent.optionsToolbarViewModel.controllersListView?.controllersListViewModel.StopRefreshTimer();
            eramViewModel.OptionsToolbarContent.optionsToolbarViewModel.aircraftListToolbarView?.aircraftListToolbarViewModel.StopRefreshTimer();
            if (eramViewModel.RadarViewModel.vatsimDataService != null)
            {
                eramViewModel.RadarViewModel.vatsimDataService.Stop();
            }
            loadProfileView.ShowDialog();
        }

        public async void InitializeGeneralSettings()
        {
            generalSettingsView = new GeneralSettingsView(eramViewModel);
            generalSettingsView = null;
        }

        public void OpenGeneralSettingsWindow()
        {
            if (generalSettingsView == null)
            {
                generalSettingsView = new GeneralSettingsView(eramViewModel);
                generalSettingsView.Owner = this;
                generalSettingsView.Closed += (_, __) => generalSettingsView = null;
                generalSettingsView.Show();
                return;
            }
        }

        public void OpenActivateSectorWindow()
        {
            PositionsToolbarView positionsToolbarView = new PositionsToolbarView(eramViewModel);
            positionsToolbarView.Owner = eramViewModel.eramView;
            positionsToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            positionsToolbarView.ShowDialog();
        }

        public void HideCursor()
        {
            if (this.Cursor == Cursors.None) return;
            this.Cursor = Cursors.None;
        }

        public void ShowCursor()
        {
            this.Cursor = Cursors.Arrow;
        }

        public void ToggleResizeBorder()
        {
            if (this.BorderThickness == new Thickness(3))
            {
                this.BorderThickness = new Thickness(1);
            }
            else
            {
                this.BorderThickness = new Thickness(3);
            }
        }

        public void ToggleTitleBar()
        {
            if (this.WindowStyle == WindowStyle.SingleBorderWindow)
            {
                this.WindowStyle = WindowStyle.None;
            }
            else
            {
                this.WindowStyle = WindowStyle.SingleBorderWindow;
            }
        }

        // ========================================================
        //                PRIVATE METHODS
        // ========================================================

        private void InitializeCursor(EramViewModel eramViewModel)
        {
            //UpdateCursorSize((int)profile.AppearanceSettings["CursorSize"]);

            //eramViewModel.PropertyChanged += (s, e) =>
            //{
            //    if (e.PropertyName == nameof(eramViewModel.CursorSize))
            //    {
            //        UpdateCursorSize(eramViewModel.CursorSize);
            //    }
            //};
        }

        private void UpdateCursorSize(int cursorSize)
        {
            var uri = new Uri($"pack://application:,,,/Resources/Cursors/Eram{cursorSize}.cur");
            //this.Cursor = new Cursor(Application.GetResourceStream(uri).Stream);
        }

        private void LoadWindowSettings()
        {
            JObject windowSettings = (JObject)profile.WindowSettings;
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
            if ((bool)windowSettings["IsFullscreen"])
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                ResizeMode = ResizeMode.CanResize;
                WindowState = WindowState.Normal;
            }
        }
    }
}
