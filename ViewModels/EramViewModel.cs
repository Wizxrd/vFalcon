using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Rendering;
using vFalcon.Services;
using vFalcon.Views;

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
        private CursorToolbarView cursorToolbarView;
        private BrightnessToolbarView brightnessToolbarView;
        private MapsToolbarView mapsToolbarView;
        private MapBrightnessToolbarView mapBrightnessToolbarView;

        private DispatcherTimer zuluTimer = new DispatcherTimer();
        private object masterToolbarContent;
        private ToolbarControlView toolbarContent;
        private ReplayControlsView replayControlsContent;
        private object ActiveToolbar;

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

        // ========================================================
        //                     PROPERTIES
        // ========================================================

        // -- Title Name --
        public string Title { get => title; set { title = value; OnPropertyChanged(); } }

        // -- Zulu Time --
        public string ZuluTime { get => zuluTime; set { zuluTime = value; OnPropertyChanged(); } }

        // --- Toolbar Content ---
        public ReplayControlsView ReplayControlsContent { get => replayControlsContent; set { replayControlsContent = value; OnPropertyChanged(); } }
        public object MasterToolbarContent { get => masterToolbarContent; set { masterToolbarContent = value; OnPropertyChanged(); } }
        public ToolbarControlView ToolbarControlContent { get => toolbarContent; set { toolbarContent = value; OnPropertyChanged(); } }

        // --- Toolbar Z-Index ---
        public int ToolbarRegionZIndex { get => _toolbarRegionZIndex; set { _toolbarRegionZIndex = value; OnPropertyChanged(); } }
        public int CanvasZIndex { get => canvasZIndex; set { canvasZIndex = value; OnPropertyChanged(); } }

        // --- Margins ---
        private Thickness timeMargin = new Thickness(20, 110, 0, 0);
        private Thickness toolbarControlMargin = new Thickness(90, 71, 0, 0);
        public Thickness TimeMargin { get => timeMargin; set { timeMargin = value; OnPropertyChanged(); } }
        public Thickness ToolbarControlMargin { get => toolbarControlMargin; set { toolbarControlMargin = value; OnPropertyChanged(); } }

        // --- Toolbar Control Position ---
        public double ToolbarControlLeft { get => toolbarControlLeft; set { toolbarControlLeft = value; OnPropertyChanged(); } }
        public double ToolbarControlRight { get => toolbarControlRight; set { toolbarControlRight = value; OnPropertyChanged(); } }
        public double ToolbarControlTop { get => toolbarControlTop; set { toolbarControlTop = value; OnPropertyChanged(); } }
        public double ToolbarControlBottom { get => toolbarControlBottom; set { toolbarControlBottom = value; OnPropertyChanged(); } }

        // --- Time Border Position ---
        public double TimeBorderLeft { get => timeBorderLeft; set { timeBorderLeft = value; OnPropertyChanged(); } }
        public double TimeBorderTop { get => timeBorderTop; set { timeBorderTop = value; OnPropertyChanged(); } }
        public double TimeBorderRight { get => timeBorderRight; set { timeBorderRight = value; OnPropertyChanged(); } }
        public double TimeBorderBottom { get => timeBorderBottom; set { timeBorderBottom = value; OnPropertyChanged(); } }

        // --- Toolbar State ---
        public string MasterRaiseLower { get => masterRaiseLower; set { masterRaiseLower = value; OnPropertyChanged(); } }
        public bool IsMasterToolbarOpen
        {
            get => (bool)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["MasterToolbarVisible"];
            set { profile.DisplayWindowSettings[0]["DisplaySettings"][0]["MasterToolbarVisible"] = value; OnPropertyChanged(); }
        }
        public bool IsRaiseMasterToolbar
        {
            get => (bool)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["RaiseMasterToolbar"];
            set { profile.DisplayWindowSettings[0]["DisplaySettings"][0]["RaiseMasterToolbar"] = value; OnPropertyChanged(); }
        }

        // --- Cursor ---
        public int CursorSize
        {
            get => (int)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"];
            set { profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"] = value; OnPropertyChanged(); }
        }

        // --- Background ---
        public Brush Background { get => background; set { background = value; OnPropertyChanged(); } }
        public int BackgroundValue { get => backgroundValue; set { backgroundValue = value; OnPropertyChanged(); } }
        public int BrightnessValue { get => brightnessValue; set { brightnessValue = value; OnPropertyChanged(); } }

        // --- Maps ---
        public HashSet<int> ActiveFilters { get; set; } = new HashSet<int>();
        public JArray ActiveVideoMapIds;
        public string GeoMapStatus { get => geoMapStatus; set { geoMapStatus = value; OnPropertyChanged(); } }
        public bool UseAlternateMapLayout
        {
            get => useAlternateMapLayout;
            set
            {
                useAlternateMapLayout = value;
                mapsToolbarView.RebuildFilters();
                mapBrightnessToolbarView.RebuildBrightnessBcg();
                OnPropertyChanged();
            }
        }
        public string ActiveGeoMap
        {
            get => (string)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"]["ACtiveGeoMap"];
            set { profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"]["ACtiveGeoMap"] = value; OnPropertyChanged(); }
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
        public Dictionary<int, List<ProcessedFeature>> FacilityFeatures { get; set; } = new Dictionary<int, List<ProcessedFeature>>();
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
        public ICommand SwitchProfileCommand { get; set; }
        public ICommand GeneralSettingsCommand { get; set; }
        public ICommand ActivateSectorCommand { get; set; }
        public ICommand IncreaseVelocityVectorCommand { get; set; }
        public ICommand DecreaseVelocityVectorCommand { get; set; }
        public ICommand RewindCommand { get; set; }
        public ICommand PlayPauseCommand { get; set; }
        public ICommand FastForwardCommand { get; set; }
        public ICommand ExitRecordingCommand { get; set; }

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
            Logger.Info("EramViewModel", "ERAM Starting Up");
            this.eramView = eramView;
            this.artcc = artcc;
            this.profile = profile;

            RadarViewModel = new RadarViewModel(this);

            SetTitle(this.artcc.id, string.Empty);
            StartZuluTimer();
            InitializeCommands();
            InitializeProfile();
            InitializeToolbarViews();
            InitializeMasterToolbar();
        }

        // ========================================================
        //               PUBLIC METHODS
        // ========================================================
        public void UpdateBackground()
        {
            double factor = 0.65 + 0.35 * (BrightnessValue / 100.0);
            double baseBlue = ((BackgroundValue / 60.0) * 127) * factor;
            byte blue = (byte)Math.Max(Math.Min(baseBlue, 255), 0);
            Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, blue));
        }

        public void OnMenuButtonMeasured(double buttonWidth, double buttonHeight)
        {
            if (!double.IsNaN(ToolbarControlRight))
            {
                ToolbarControlRight += buttonWidth;
                ToolbarControlBottom += buttonHeight;
            }
        }

        public void OnMasterToolbar()
        {
            IsMasterToolbarOpen = !IsMasterToolbarOpen;
            MasterToolbarContent = IsMasterToolbarOpen ? ActiveToolbar : null;
        }

        public void SwapZOrder()
        {
            if (IsRaiseMasterToolbar) SetZOrderLowered(); else SetZOrderRaised();
            OnPropertyChanged(nameof(IsRaiseMasterToolbar));
        }

        public void OnCursorCommand()
        {
            if (!cursorToolbarOpen) { ActiveToolbar = cursorToolbarView; MasterToolbarContent = cursorToolbarView; }
            else { ActiveToolbar = masterToolbarView; MasterToolbarContent = masterToolbarView; }
            cursorToolbarOpen = !cursorToolbarOpen;
        }

        private bool mapBrightnessToolbarOpen = false;
        public void OnMapBrightnessCommand()
        {
            if (!mapBrightnessToolbarOpen) { ActiveToolbar = mapBrightnessToolbarView; MasterToolbarContent = mapBrightnessToolbarView; }
            else { ActiveToolbar = masterToolbarView; MasterToolbarContent = masterToolbarView; }
            mapBrightnessToolbarOpen = !mapBrightnessToolbarOpen;
        }

        public void OnBrightnessCommand()
        {
            if (!brightnessToolbarOpen) { ActiveToolbar = brightnessToolbarView; MasterToolbarContent = brightnessToolbarView; }
            else { ActiveToolbar = masterToolbarView; MasterToolbarContent = masterToolbarView; }
            brightnessToolbarOpen = !brightnessToolbarOpen;
        }

        public void OnMapsCommand()
        {
            if (!mapsToolbarOpen) { ActiveToolbar = mapsToolbarView; MasterToolbarContent = mapsToolbarView; }
            else { ActiveToolbar = masterToolbarView; MasterToolbarContent = masterToolbarView; }
            mapsToolbarOpen = !mapsToolbarOpen;
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
        private bool isPlaybackMode = false;
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
                }
            }
            else
            {
                RecordingStatus = string.Empty;
                IsRecording = false;
                recordingTimer.Stop();
                recordingStopwatch.Stop();
                RadarViewModel.pilotService.recordingService.Stop();
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
            SetTitle(this.artcc.id, string.Empty);
        }

        public void OnLoadRecording(string recordingPath)
        {
            RadarViewModel.vatsimDataService.Stop();
            RadarViewModel.pilotService.Pilots.Clear();
            RadarViewModel.Redraw();
            playbackService.StartPlayback(this, profile, recordingPath);
            ReplayControlVisibility = Visibility.Visible;
            if (replayControlsContent.DataContext is ReplayControlsViewModel rcvm)
            {
                rcvm.MaximumSliderValue = playbackService.GetTotalTickCount();
            }
            isPlaybackMode = true;
            SetTitle(this.artcc.id, string.Empty);
            RadarViewModel.Redraw();
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

        // ========================================================
        //             PRIVATE METHODS
        // =====S===================================================

        private void SetTitle(string artcc, string sector)
        {
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
            MasterToolbarCommand = new RelayCommand(OnMasterToolbar);
            SwapZOrderCommand = new RelayCommand(SwapZOrder);
            CursorCommand = new RelayCommand(OnCursorCommand);
            BrightnessCommand = new RelayCommand(OnBrightnessCommand);
            MapsCommand = new RelayCommand(OnMapsCommand);
            MapBrightnessCommand = new RelayCommand(OnMapBrightnessCommand);
            ToggleTdmCommand = new RelayCommand(() => { TdmActive = !TdmActive; RadarViewModel.Redraw(); });
            LoadRecordingCommand = new RelayCommand(() => LoadRecordingAction?.Invoke());
            StartStopRecordingCommand = new RelayCommand(() => StartStopRecordingAction?.Invoke());
            SwitchProfileCommand = new RelayCommand(OnSwitchProfileCommand);
            GeneralSettingsCommand = new RelayCommand(OnGeneralSettingsCommand);
            ActivateSectorCommand = new RelayCommand(OnActivateSectorCommand);
            IncreaseVelocityVectorCommand = new RelayCommand(OnIncreaseVelocityVectorCommand);
            DecreaseVelocityVectorCommand = new RelayCommand(OnDecreaseVelocityVectorCommand);
            RewindCommand = new RelayCommand(OnRewindCommand);
            PlayPauseCommand = new RelayCommand(OnPlayPauseCommand);
            FastForwardCommand = new RelayCommand(OnFastForwardCommand);
            ExitRecordingCommand = new RelayCommand(() => ExitRecording());
        }

        private async Task InitializeProfile()
        {
            ActiveGeoMap = (string)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["ActiveGeoMap"] ?? null;
            VelocityVector = 1;
            LoadMapFilters();
            LoadBrightness();
            LoadTimeSettings();
            LoadToolbarControlMenu();
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
            if (ActiveToolbar == mapsToolbarView)
            {
                mapsToolbarView = new MapsToolbarView(this);
                MasterToolbarContent = mapsToolbarView;
                ActiveToolbar = mapsToolbarView;
            }
            else
            {
                mapsToolbarView = new MapsToolbarView(this);
            }

            if (ActiveToolbar == brightnessToolbarView)
            {
                bool mapBrightnessOpen = false;

                if (brightnessToolbarView.DataContext is BrightnessToolbarViewModel oldVm)
                {
                    mapBrightnessOpen = oldVm.MapBrightnessOpen;
                }
                brightnessToolbarView = new BrightnessToolbarView(this);
                if (brightnessToolbarView.DataContext is BrightnessToolbarViewModel newVm)
                {
                    if (mapBrightnessOpen) newVm.OpenMapBrightness();
                }
                MasterToolbarContent = brightnessToolbarView;
                ActiveToolbar = brightnessToolbarView;
            }
            else
            {
                brightnessToolbarView = new BrightnessToolbarView(this);
            }
            LoadGeoMaps();
            await LoadVideoMaps();
            ActiveFilters.Add(0);
            RadarViewModel.Redraw();
        }

        private void InitializeToolbarViews()
        {
            masterToolbarView = new MasterToolbarView(this);
            cursorToolbarView = new CursorToolbarView(this);
            brightnessToolbarView = new BrightnessToolbarView(this);
            mapsToolbarView = new MapsToolbarView(this);
            mapBrightnessToolbarView = new MapBrightnessToolbarView(this);
            ReplayControlsContent = new ReplayControlsView(this);

        }

        private void InitializeMasterToolbar()
        {
            ToolbarControlContent = new ToolbarControlView(this);
            MasterRaiseLower = IsRaiseMasterToolbar ? "LOWER" : "RAISE";
            IsMasterToolbarOpen = !IsMasterToolbarOpen;
            ActiveToolbar = masterToolbarView;
            OnMasterToolbar();
            if (IsRaiseMasterToolbar) SetZOrderRaised(); else SetZOrderLowered();
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
            string rawFilters = profile.DisplayWindowSettings[0]["DisplaySettings"][0]["MapFilters"]?.ToString();
            if (rawFilters.ToLower() != "none" || !string.IsNullOrEmpty(rawFilters))
            {
                foreach (var token in rawFilters.Split(','))
                {
                    string trimmed = token.Trim();
                    if (trimmed.StartsWith("Map", StringComparison.OrdinalIgnoreCase) && int.TryParse(trimmed.Substring(3), out int filterNum))
                    {
                        ActiveFilters.Add(filterNum);
                    }
                }
            }
            RadarViewModel.Redraw();
        }

        private void LoadBrightness()
        {
            BackgroundValue = (int)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"]["Background"];
            BrightnessValue = (int)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"]["SystemBrightness"];
            UpdateBackground();
        }

        private void LoadTimeSettings()
        {
            JObject displaySettings = (JObject)profile.DisplayWindowSettings[0]["DisplaySettings"][0];
            JObject timeViewSettings = (JObject)displaySettings["TimeViewSettings"];

            double[] parts = ((string)timeViewSettings["Location"]["Location"]).Split(',').Select(s => double.Parse(s, CultureInfo.InvariantCulture)).ToArray();

            string anchor = (string)timeViewSettings["Location"]["Anchor"];
            if (parts[0] == TimeMargin.Left && parts[1] == TimeMargin.Top && anchor == "TopLeft") return;

            TimeMargin = new Thickness(0);
            TimeBorderRight = TimeBorderBottom = double.NaN;

            switch (anchor)
            {
                case "TopLeft":
                    TimeBorderLeft = parts[0];
                    TimeBorderTop = parts[1];
                    break;
                case "TopRight":
                    TimeBorderRight = parts[0];
                    TimeBorderTop = parts[1];
                    TimeBorderLeft = double.NaN;
                    break;
                case "BottomLeft":
                    TimeBorderLeft = parts[0];
                    TimeBorderBottom += parts[1];
                    TimeBorderTop = double.NaN;
                    break;
                case "BottomRight":
                    TimeBorderRight = parts[0];
                    TimeBorderBottom += parts[1];
                    TimeBorderTop = TimeBorderLeft = double.NaN;
                    break;
            }
        }

        private void LoadToolbarControlMenu()
        {
            JObject displaySettings = (JObject)profile.DisplayWindowSettings[0]["DisplaySettings"][0];
            JArray tearoffs = (JArray)displaySettings["Tearoffs"];
            if (tearoffs.Count == 0) return;

            foreach (JObject tearoff in tearoffs)
            {
                if ((string)tearoff["Type"] == "ToolbarControlMenu")
                {
                    double[] parts = ((string)tearoff["Location"]["Location"])
                        .Split(',')
                        .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                        .ToArray();

                    string anchor = (string)tearoff["Location"]["Anchor"];
                    if (parts[0] == ToolbarControlMargin.Left && parts[1] == ToolbarControlMargin.Top && anchor == "TopLeft") return;

                    ToolbarControlMargin = new Thickness(0);
                    switch (anchor)
                    {
                        case "TopLeft":
                            ToolbarControlLeft = parts[0];
                            ToolbarControlTop = parts[1];
                            break;
                        case "TopRight":
                            ToolbarControlRight += parts[0];
                            ToolbarControlTop = parts[1];
                            ToolbarControlLeft = double.NaN;
                            break;
                        case "BottomLeft":
                            ToolbarControlLeft = parts[0];
                            ToolbarControlBottom = parts[1];
                            ToolbarControlTop = double.NaN;
                            break;
                        case "BottomRight":
                            ToolbarControlRight += parts[0];
                            ToolbarControlBottom = parts[1];
                            ToolbarControlTop = ToolbarControlLeft = double.NaN;
                            break;
                    }
                    return;
                }
            }
        }

        private void LoadGeoMaps()
        {
            ArtccGeoMaps = (JArray)artcc.facility["eramConfiguration"]["geoMaps"];
            if (ActiveGeoMap == null)
            {
                // no geo map set, so we will use the default first one in facility file, same as CRC
                JObject defaultGeoMap = (JObject)artcc.facility["eramConfiguration"]["geoMaps"][0];
                ActiveGeoMap = (string)defaultGeoMap["name"];
                ActiveVideoMapIds = (JArray)defaultGeoMap["videoMapIds"];
                MapsLabelLine1 = (string)defaultGeoMap["labelLine1"];
                MapsLabelLine2 = (string)defaultGeoMap["labelLine2"];
                return;
            }
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
            FacilityFeatures = await GeoMap.LoadFacilityFeatures(artcc, ActiveVideoMapIds);
            RadarViewModel.InitializeVatsimServices();
            GeoMapStatus = string.Empty;
            Logger.Info("EramViewModel.LoadVideoMaps", "ERAM Ready");
        }

        private void SetZOrderRaised()
        {
            ToolbarRegionZIndex = 2;
            CanvasZIndex = 1;
            IsRaiseMasterToolbar = true;
            MasterRaiseLower = "LOWER";
            OnPropertyChanged(nameof(MasterRaiseLower));
        }

        private void SetZOrderLowered()
        {
            CanvasZIndex = 2;
            ToolbarRegionZIndex = 1;
            IsRaiseMasterToolbar = false;
            MasterRaiseLower = "RAISE";
            OnPropertyChanged(nameof(MasterRaiseLower));
        }
    }
}
