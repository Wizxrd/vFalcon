using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using vFalcon.Audio;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Nasr;
using vFalcon.Rendering;
using vFalcon.Services;
using vFalcon.Services.Service;
using vFalcon.Views;
using MessageBox = vFalcon.Services.Service.MessageBoxService;

namespace vFalcon.ViewModels
{
    public class EramViewModel : ViewModelBase
    {
        // ========================================================
        //                     FIELDS
        // ========================================================
        public EramView eramView;
        public Artcc artcc;
        public Profile profile;

        private MasterToolbarView masterToolbarView;
        private OptionsToolbarView optionsToolbarContent;

        private DispatcherTimer zuluTimer = new DispatcherTimer();
        private object masterToolbarContent;
        private ReplayControlsView replayControlsContent;
        private object ActiveToolbar;

        public Dictionary<string, string> ActivatedSectors = new Dictionary<string, string>();

        private Brush background;

        private string title = "vFalcon";
        private string geoMapStatus;
        private string zuluTime;

        private string masterRaiseLower;
        private string mapsLabelLine1;
        private string mapsLabelLine2;
        private string zoomLevel;

        private bool cursorToolbarOpen;
        private bool brightnessToolbarOpen;
        private bool mapsToolbarOpen;
        private bool tdmActive;
        private bool useAlternateMapLayout = false;
        private bool sectorActivated = false;

        private double timeBorderLeft;
        private double timeBorderTop = double.NaN;
        private double timeBorderRight = double.NaN;
        private double timeBorderBottom = double.NaN;

        private double toolbarControlLeft;
        private double toolbarControlTop;
        private double toolbarControlRight;
        private double toolbarControlBottom;

        private int _toolbarRegionZIndex = 1;
        private int canvasZIndex = 2;
        private int backgroundValue;
        private int brightnessValue;

        private int velocityVector = 1;

        public float DatablockTextSize = 12f;
        public double FullDatablockBrightness = 100.0;
        public double LimitedDatablockBrightness = 75.0;
        public double HistoryBrightness = 25.0;
        public int HistoryCount = 6;
        public int MapBrightness = 100;

        private string mouseCoordinates;
        public bool ShowLatLon = false;

        // ========================================================
        //                     PROPERTIES
        // ========================================================

        public string MouseCoordinates
        {
            get => mouseCoordinates;
            set
            {
                mouseCoordinates = value;
                OnPropertyChanged();
            }
        }

        // -- Title Name --
        public string Title { get => title; set { title = value; OnPropertyChanged(); } }

        // -- Zulu Time --
        public string ZuluTime { get => zuluTime; set { zuluTime = value; OnPropertyChanged(); } }

        // --- Toolbar Content ---
        public ReplayControlsView ReplayControlsContent { get => replayControlsContent; set { replayControlsContent = value; OnPropertyChanged(); } }
        public OptionsToolbarView OptionsToolbarContent { get => optionsToolbarContent; set { optionsToolbarContent = value; OnPropertyChanged(); } }
        public object MasterToolbarContent { get => masterToolbarContent; set { masterToolbarContent = value; OnPropertyChanged(); } }

        // --- Background ---
        public Brush Background { get => background; set { background = value; OnPropertyChanged(); } }
        public int BackgroundValue { get => backgroundValue; set { backgroundValue = value; OnPropertyChanged(); } }
        public int BrightnessValue { get => brightnessValue; set { brightnessValue = value; OnPropertyChanged(); } }

        // --- Maps ---
        public HashSet<int> ActiveFilters { get; set; } = new HashSet<int>();
        public HashSet<string> StarsActiveFilters { get; set; } = new HashSet<string>();
        public JArray ActiveVideoMapIds;
        public string GeoMapStatus { get => geoMapStatus; set { geoMapStatus = value; OnPropertyChanged(); } }
        public bool UseAlternateMapLayout
        {
            get => useAlternateMapLayout;
            set
            {
                useAlternateMapLayout = value;
                OnPropertyChanged();
            }
        }
        public string ActiveGeoMap
        {
            get => (string)profile.ActiveGeoMap;
            set { profile.ActiveGeoMap = value; OnPropertyChanged(); }
        }
        public JArray ArtccGeoMaps
        {
            get => (JArray)artcc.facility["eramConfiguration"]["geoMaps"];
            set { artcc.facility["eramConfiguration"]["geoMaps"] = value; OnPropertyChanged(); }
        }
        public string MapsLabelLine1 { get => mapsLabelLine1; set { mapsLabelLine1 = value; OnPropertyChanged(); } }
        public string MapsLabelLine2 { get => mapsLabelLine2; set { mapsLabelLine2 = value; OnPropertyChanged(); } }

        // --- Radar + Filters ---
        public RadarViewModel RadarViewModel { get; set; }
        public Dictionary<string, List<ProcessedFeature>> FacilityFeatures { get; set; } = new Dictionary<string, List<ProcessedFeature>>();
        public string ZoomLevel { get => zoomLevel; set { zoomLevel = value; OnPropertyChanged(); } }

        // --- TDM ---
        public bool TdmActive
        {
            get => tdmActive;
            set
            {
                if (tdmActive != value)
                {
                    tdmActive = value;
                    RadarViewModel.Redraw();
                    OnPropertyChanged(nameof(TdmActive)); // ensure bindings update
                }
            }
        }

        public bool SectorActivated
        {
            get => sectorActivated;
            set
            {
                sectorActivated = value;
                OnPropertyChanged();
            }
        }

        public int VelocityVector
        {
            get => velocityVector;
            set
            {
                velocityVector = value;
                OnPropertyChanged();
            }
        }

        private Visibility replayControlVisibility = Visibility.Collapsed;
        public Visibility ReplayControlVisibility
        {
            get => replayControlVisibility;
            set
            {
                replayControlVisibility = value;
                OnPropertyChanged();
            }
        }

        // ========================================================
        //                     COMMANDS
        // ========================================================
        public ICommand ToolbarControlCommand { get; set; }
        public ICommand MasterToolbarCommand { get; set; }
        public ICommand SwapZOrderCommand { get; set; }
        public ICommand CursorCommand { get; set; }
        public ICommand BrightnessCommand { get; set; }
        public ICommand MapsCommand { get; set; }
        public ICommand MapBrightnessCommand { get; set; }
        public ICommand ToggleTdmCommand { get; set; }
        public ICommand LoadRecordingCommand { get; set; }
        public ICommand StartStopRecordingCommand { get; set; }

        public ICommand SaveProfileCommand { get; set; }
        public ICommand SaveProfileAsCommand { get; set; }
        public ICommand SwitchProfileCommand { get; set; }
        public ICommand GeneralSettingsCommand { get; set; }
        public ICommand ActivateSectorCommand { get; set; }
        public ICommand IncreaseVelocityVectorCommand { get; set; }
        public ICommand DecreaseVelocityVectorCommand { get; set; }
        public ICommand RewindCommand { get; set; }
        public ICommand PlayPauseCommand { get; set; }
        public ICommand FastForwardCommand { get; set; }
        public ICommand ExitRecordingCommand { get; set; }

        public ICommand ClearFindCommand { get; set; }

        public ICommand OpenSearchWindow { get; set; }
        public ICommand OpenFindWindow { get; set; }

        public ICommand CaptureScreenCommand { get; set; }

        public ICommand ToggleResizeBorderCommand { get; set; }

        public ICommand ToggleTitleBarCommand { get; set; }

        public ICommand FullscreenCommand { get; set; }

        // ========================================================
        //                     ACTIONS
        // ========================================================
        public event Action? LoadRecordingAction;
        public event Action? SwitchProfileAction;
        public event Action? GeneralSettingsAction;
        public event Action? ActivateSectorAction;
        public event Action? StartStopRecordingAction;
        public event Action? ExitRecordingAction;

        // ========================================================
        //                 CONSTRUCTOR
        // ========================================================

        public EramViewModel(EramView eramView, Artcc artcc, Profile profile)
        {
            Logger.Info("Display", "Starting");
            this.eramView = eramView;
            this.artcc = artcc;
            this.profile = profile;

            RadarViewModel = new RadarViewModel(this);
            Logger.LogLevelThreshold = Enum.IsDefined(typeof(LogLevel), profile.LogLevel) ? (LogLevel)profile.LogLevel : LogLevel.Info;
            SetTitle(this.artcc.id, string.Empty);
            StartZuluTimer();
            InitializeCommands();
            InitializeProfile();
            InitializeToolbarViews();
        }

        // ========================================================
        //               PUBLIC METHODS
        // ========================================================

        public void OnCaptureScreenCommand()
        {
            UIElement element = eramView;
            var dpi = VisualTreeHelper.GetDpi(eramView);
            var size = new Size(((FrameworkElement)element).ActualWidth, ((FrameworkElement)element).ActualHeight);
            var rtb = new RenderTargetBitmap((int)(size.Width * dpi.DpiScaleX), (int)(size.Height * dpi.DpiScaleY), dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen()) dc.DrawRectangle(new VisualBrush(element), null, new Rect(new Point(), size));
            rtb.Render(dv);
            var frame = BitmapFrame.Create(rtb);

            var dlg = new SaveFileDialog
            {
                Title = "Save Screen Capture",
                Filter = "PNG Image (*.png)|*.png",
                DefaultExt = ".png",
                AddExtension = true,
                OverwritePrompt = true
            };

            if (dlg.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(frame);
                using var fs = File.Create(dlg.FileName);
                encoder.Save(fs);
            }
        }

        public void EramMouseMove(SKPoint skPoint)
        {
            MapState mapState = RadarViewModel.mapState;
            System.Drawing.Size size = new System.Drawing.Size(mapState.Width, mapState.Height);
            SKPoint coords = ScreenMap.ScreenToCoordinate(size, mapState.Scale, mapState.PanOffset, skPoint);
            MouseCoordinates = $"{coords.X:F2}, {coords.Y:F2}";
        }

        public void UpdateBackground()
        {
            double factor = 0.65 + 0.35 * (BrightnessValue / 100.0);
            double baseBlue = ((BackgroundValue / 60.0) * 127) * factor;
            byte blue = (byte)Math.Max(Math.Min(baseBlue, 255), 0);
            Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, blue));
        }

        private bool mapBrightnessToolbarOpen = false;

        public void OnMapsCommand()
        {
        }

        ProfileService profileService = new();
        public void OnSaveProfileCommand()
        {
            var confirmed = MessageBox.Confirm($"Save profile: \"{profile.Name}\"?");
            if (!confirmed) return;
            profileService.Save(profile);
        }

        public void OnSaveProfileAsCommand()
        {
            SaveProfileAsView saveProfileAsView = new(this);
            saveProfileAsView.Owner = eramView;
            saveProfileAsView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            saveProfileAsView.ShowDialog();
        }

        public void OnSwitchProfileCommand()
        {
            SwitchProfileAction?.Invoke();
        }

        public void OnGeneralSettingsCommand()
        {
            GeneralSettingsAction?.Invoke();
        }

        public void OnActivateSectorCommand()
        {
            ActivateSectorAction?.Invoke();
        }

        public void OnIncreaseVelocityVectorCommand()
        {
            masterToolbarView?.IncreaseVelocityVector();
        }

        public void OnDecreaseVelocityVectorCommand()
        {
            masterToolbarView?.DecreaseVelocityVector();
        }

        private string recordingStatus = string.Empty;
        public bool isRecording = false;
        public bool isPlaybackMode = false;
        private DispatcherTimer recordingTimer = new DispatcherTimer();
        private Stopwatch recordingStopwatch = new Stopwatch();

        public string RecordingStatus
        {
            get => recordingStatus;
            set
            {
                recordingStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsRecording
        {
            get => isRecording;
            set
            {
                isRecording = value;
                OnPropertyChanged();
            }
        }

        public AudioLoopback AudioLoopback = new AudioLoopback();
        public MicRecorder MicRecorder = new MicRecorder();
        public void OnToggleRecording()
        {
            if (isPlaybackMode) return;
            if (!IsRecording)
            {
                if (RadarViewModel.pilotService == null) return;
                if (RadarViewModel.pilotService.recordingService != null)
                {
                    IsRecording = true;
                    recordingTimer.Interval = TimeSpan.FromSeconds(1);
                    recordingTimer.Tick += OnRecordingTick;
                    recordingTimer.Start();
                    recordingStopwatch.Restart();
                    RadarViewModel.pilotService.recordingService.Start();
                    //if (profile.RecordAudio)
                    //{
                    //    AudioLoopback.Start(Loader.LoadFolder("AudioRecordings\\Output") + $"\\{RadarViewModel.pilotService.recordingService.recordingName}.wav");
                    //    int? vk = profile.PttKey;
                    //    if (vk.HasValue && vk.Value > 0)
                    //    {
                    //        MicRecorder.Start(Loader.LoadFolder("AudioRecordings\\Input") + $"\\{RadarViewModel.pilotService.recordingService.recordingName}.wav");
                    //    }
                    //}
                }
            }
            else
            {
                RecordingStatus = string.Empty;
                IsRecording = false;
                recordingTimer.Stop();
                recordingStopwatch.Stop();
                RadarViewModel.pilotService.recordingService.Stop();
                //MicRecorder.Stop();
                //AudioLoopback.Stop();
            }
        }

        private void OnRecordingTick(object? sender, EventArgs? e)
        {
            var elapsed = recordingStopwatch.Elapsed;
            RecordingStatus = $"Recording {elapsed:hh\\:mm\\:ss}";
        }

        public PlaybackService playbackService = new();

        public void UpdateReplayControls(int tick)
        {
            if (replayControlsContent.DataContext is ReplayControlsViewModel rcvm)
            {
                rcvm.SliderValueTick = tick;
                var ts = TimeSpan.FromSeconds(tick*15);
                rcvm.ElapsedTimeTick = $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";

            }
        }

        public void ExitRecording()
        {
            playbackService.StopPlayback();
            ReplayControlVisibility = Visibility.Collapsed;
            RadarViewModel.vatsimDataService.Start();
            RadarViewModel.Redraw();
            ExitRecordingAction?.Invoke();
            isPlaybackMode = false;
            RadarViewModel.routeService = new(null);
            findOptionsToolbarView = new FindOptionsToolbarView(this);
            SetTitle(this.artcc.id, string.Empty);
        }

        public async void OnLoadRecording(string recordingPath)
        {
            try
            {
                ReplayControlsContent = new ReplayControlsView(this);
                if (RadarViewModel.vatsimDataService != null) RadarViewModel.vatsimDataService.Stop();
                RadarViewModel.pilotService.Pilots.Clear();
                RadarViewModel.Redraw();
                GeoMapStatus = "PREPARING NAV DATA";
                playbackService.StopPlayback();
                playbackService = new();
                JObject replayJson = playbackService.SetRecordingPath(recordingPath);
                JObject navData = (JObject)playbackService.replayJson["NavData"];
                NavData nasr = new NavData(null)
                {
                    Date = (string)navData["Date"],
                    ForceDownload = true,
                    SwapNavDate = true,
                    Delay = 250
                };
                await nasr.Run();
                RadarViewModel.routeService = new((string)navData["Date"]);
                findOptionsToolbarView = new FindOptionsToolbarView(this);
                GeoMapStatus = string.Empty;
                playbackService.StartPlayback(this, profile);
                ReplayControlVisibility = Visibility.Visible;
                if (replayControlsContent.DataContext is ReplayControlsViewModel rcvm)
                {
                    rcvm.MaximumSliderValue = playbackService.GetTotalTickCount();
                }
                isPlaybackMode = true;
                SetTitle(this.artcc.id, string.Empty);
                RadarViewModel.Redraw();
            }
            catch (Exception ex)
            {
                Logger.Error("OnLoadRecording", ex.ToString());
            }
        }

        public void OnRewindCommand()
        {
            if (isPlaybackMode)
            {
                replayControlsContent.RewindButtonClick(null, null);
            }
        }

        public void OnPlayPauseCommand()
        {
            if (isPlaybackMode)
            {
                replayControlsContent.PlayPauseButtonClick(null, null);
            }
        }

        public void OnFastForwardCommand()
        {
            if (isPlaybackMode)
            {
                replayControlsContent.FastForwardButtonClick(null, null);
            }
        }

        private void OnFullscreenCommand()
        {
            if (eramView.WindowState == WindowState.Maximized)
            {
                eramView.WindowStyle = WindowStyle.SingleBorderWindow;
                eramView.ResizeMode = ResizeMode.CanResize;
                eramView.WindowState = WindowState.Normal;
                profile.WindowSettings["IsFullscreen"] = false;
            }
            else
            {
                eramView.WindowStyle = WindowStyle.None;
                eramView.ResizeMode = ResizeMode.NoResize;
                eramView.WindowState = WindowState.Maximized;
                profile.WindowSettings["IsFullscreen"] = true;
            }
        }

        // ========================================================
        //             PRIVATE METHODS
        // =====S===================================================

        private void SetTitle(string artcc, string sector)
        {
            if (profile.DisplayType == "STARS") artcc = profile.FacilityId;
            if (sector == string.Empty)
            {
                if (isPlaybackMode)
                {
                    Title = $"vFalcon : {artcc} : Playback Mode";
                    return;
                }
                Title = $"vFalcon : {artcc} : Live Mode";
            }
            else
            {
                if (isPlaybackMode)
                {
                    Title = $"vFalcon : {artcc} : {sector} : Playback Mode";
                    return;
                }
                Title = $"vFalcon : {artcc} : {sector} : Live Mode";
            }
        }

        private void StartZuluTimer()
        {
            ZuluTimerTick(null, null);
            zuluTimer.Interval = TimeSpan.FromMilliseconds(500);
            zuluTimer.Tick += ZuluTimerTick;
            zuluTimer.Start();
        }

        private void ZuluTimerTick(object? sender, EventArgs? e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ZuluTime = DateTime.UtcNow.ToString("HHmm ss");
            }, DispatcherPriority.Normal);
        }

        // ========================================================
        //             INITIALIZATION METHODS
        // ========================================================
        private void InitializeCommands()
        {
            MapsCommand = new RelayCommand(OnMapsCommand);
            ToggleTdmCommand = new RelayCommand(() => { TdmActive = !TdmActive; RadarViewModel.Redraw(); });
            LoadRecordingCommand = new RelayCommand(() => LoadRecordingAction?.Invoke());
            StartStopRecordingCommand = new RelayCommand(() => optionsToolbarContent.ToggleRecordingTexts());
            SaveProfileCommand = new RelayCommand(OnSaveProfileCommand);
            SaveProfileAsCommand = new RelayCommand(OnSaveProfileAsCommand);
            SwitchProfileCommand = new RelayCommand(OnSwitchProfileCommand);
            GeneralSettingsCommand = new RelayCommand(OnGeneralSettingsCommand);
            ActivateSectorCommand = new RelayCommand(OnActivateSectorCommand);
            IncreaseVelocityVectorCommand = new RelayCommand(OnIncreaseVelocityVectorCommand);
            DecreaseVelocityVectorCommand = new RelayCommand(OnDecreaseVelocityVectorCommand);
            RewindCommand = new RelayCommand(OnRewindCommand);
            PlayPauseCommand = new RelayCommand(OnPlayPauseCommand);
            FastForwardCommand = new RelayCommand(OnFastForwardCommand);
            ExitRecordingCommand = new RelayCommand(() => ExitRecording());
            ClearFindCommand = new RelayCommand(OnClearFindCommand);
            OpenFindWindow = new RelayCommand(OnOpenFindWindow);
            OpenSearchWindow = new RelayCommand(OnOpenSearchWindow);
            CaptureScreenCommand = new RelayCommand(OnCaptureScreenCommand);
            ToggleResizeBorderCommand = new RelayCommand(OnToggleResizeBorderCommand);
            ToggleTitleBarCommand = new RelayCommand(OnToggleTitleBarCommand);
            FullscreenCommand = new RelayCommand(OnFullscreenCommand);
        }

        private void OnToggleTitleBarCommand()
        {
            eramView.ToggleTitleBar();
        }

        private void OnToggleResizeBorderCommand()
        {
            eramView.ToggleResizeBorder();
        }

        FindOptionsToolbarView findOptionsToolbarView;
        private void OnOpenFindWindow()
        {
            findOptionsToolbarView = new FindOptionsToolbarView(this);
            findOptionsToolbarView.Owner = eramView;
            findOptionsToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            findOptionsToolbarView.ShowDialog();
        }

        private void OnOpenSearchWindow()
        {
            SearchOptionsToolbarView searchOptionsToolbarView = new SearchOptionsToolbarView(this);
            searchOptionsToolbarView.Owner = eramView;
            searchOptionsToolbarView.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            searchOptionsToolbarView.ShowDialog();
        }

        private void OnClearFindCommand()
        {
            RadarViewModel.Find = false;
            RadarViewModel.FindCoords = null;
            RadarViewModel.Redraw();
        }

        private async Task InitializeProfile()
        {
            ActiveGeoMap = profile.ActiveGeoMap;
            VelocityVector = 1;
            LoadMapFilters();
            LoadBrightness();
            LoadGeoMaps();
            await LoadVideoMaps();
            InitializeZoom();
            ActiveFilters.Add(0);
            RadarViewModel.Redraw();
        }

        public async Task SwapGeoMapSet()
        {
            FacilityFeatures.Clear();
            ActiveFilters.Clear();
            RadarViewModel.Redraw();
            OptionsToolbarContent.MapsOptionsToolbarView.RebuildFilters();
            LoadGeoMaps();
            await LoadVideoMaps();
            ActiveFilters.Add(0);
            RadarViewModel.Redraw();
        }

        private void InitializeToolbarViews()
        {
            masterToolbarView = new MasterToolbarView(this);
            ReplayControlsContent = new ReplayControlsView(this);
            OptionsToolbarContent = new OptionsToolbarView(this);
        }

        private void InitializeZoom()
        {
            ZoomLevel = RadarViewModel.GetCurrentZoomString();
            RadarViewModel.ZoomLevelChanged = zoom =>
            {
                ZoomLevel = zoom;
            };
        }

        private void LoadMapFilters()
        {
            foreach (var filter in profile.MapFilters)
            {
                if (profile.DisplayType == "ERAM")
                {
                    ActiveFilters.Add((int)filter.Value);
                }
                else if (profile.DisplayType == "STARS")
                {
                    StarsActiveFilters.Add(filter.Key);
                }
            }
            RadarViewModel.Redraw();
        }

        private void LoadBrightness()
        {
            BackgroundValue = (int)profile.AppearanceSettings["Background"];
            BrightnessValue = (int)profile.AppearanceSettings["Backlight"];
            UpdateBackground();
        }

        private void LoadGeoMaps()
        {
            if (ActiveGeoMap == null)
            {
                if (profile.DisplayType == "ERAM")
                {
                    // no geo map set, so we will use the default first one in facility file, same as CRC
                    JObject defaultGeoMap = (JObject)artcc.facility["eramConfiguration"]["geoMaps"][0];
                    ActiveGeoMap = (string)defaultGeoMap["name"];
                    ActiveVideoMapIds = (JArray)defaultGeoMap["videoMapIds"];
                    MapsLabelLine1 = (string)defaultGeoMap["labelLine1"];
                    MapsLabelLine2 = (string)defaultGeoMap["labelLine2"];
                }
                else if (profile.DisplayType == "STARS")
                {
                    JArray facilities = (JArray)artcc.facility["childFacilities"];
                    foreach (JObject facility in facilities)
                    {
                        if ((string)facility["id"] == profile.FacilityId)
                        {
                            JObject childFacilities = (JObject)facility["starsConfiguration"];
                            ActiveVideoMapIds = (JArray)childFacilities["videoMapIds"];
                            return;
                        }
                    }
                }
                return;
            }
            else if (profile.DisplayType == "ERAM")
            {
                ArtccGeoMaps = (JArray)artcc.facility["eramConfiguration"]["geoMaps"];
                foreach (JObject geoMap in ArtccGeoMaps)
                {
                    string name = (string)geoMap["name"];
                    if (name == ActiveGeoMap)
                    {
                        ActiveVideoMapIds = (JArray)geoMap["videoMapIds"];
                        MapsLabelLine1 = (string)geoMap["labelLine1"];
                        MapsLabelLine2 = (string)geoMap["labelLine2"];
                        break;
                    }
                }
            }
            else if (profile.DisplayType == "STARS")
            {
                JArray facilities = (JArray)artcc.facility["childFacilities"];
                foreach (JObject facility in facilities)
                {
                    if ((string)facility["id"] == profile.FacilityId)
                    {
                        JObject childFacilities = (JObject)facility["starsConfiguration"];
                        ActiveVideoMapIds = (JArray)childFacilities["videoMapIds"];
                        return;
                    }
                }
            }
        }

        private async Task LoadVideoMaps()
        {
            if (RadarViewModel.PilotsRendering)
            {
                GeoMapStatus = "PREPARING GEO MAPS";
            }
            else
            {
                geoMapStatus = "PREPARING GEO MAPS & SURVEILLANCE DATA";
            }

            if (profile.DisplayType == "ERAM")
            {
                FacilityFeatures = await GeoMap.LoadFacilityFeatures(artcc, ActiveVideoMapIds);
            }
            else if (profile.DisplayType == "STARS")
            {
                FacilityFeatures = await GeoMap.LoadFacilityFeaturesStars(artcc, ActiveVideoMapIds);
            }
            RadarViewModel.InitializeVatsimServices();
            GeoMapStatus = string.Empty;
            Logger.Info("Display", "Ready");
            eramView.InitializeGeneralSettings();
        }
    }
}
