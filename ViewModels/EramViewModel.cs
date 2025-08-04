using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Views;

namespace vFalcon.ViewModels
{
    public class EramViewModel : ViewModelBase
    {
        // ========================================================
        //                     FIELDS
        // ========================================================
        public Artcc artcc;
        public Profile profile;

        private MasterToolbarView masterToolbarView;
        private CursorToolbarView cursorToolbarView;
        private BrightnessToolbarView brightnessToolbarView;
        private MapsToolbarView mapsToolbarView;

        private DispatcherTimer zuluTimer = new DispatcherTimer();
        private object masterToolbarContent;
        private ToolbarControlView toolbarContent;
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

        // ========================================================
        //                     PROPERTIES
        // ========================================================

        // -- Title Name --
        public string Title { get => title; set { title = value; OnPropertyChanged(); } }

        // -- Zulu Time --
        public string ZuluTime { get => zuluTime; set { zuluTime = value; OnPropertyChanged(); } }

        // --- Toolbar Content ---
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
        public bool UseAlternateMapLayout { get => useAlternateMapLayout; set { useAlternateMapLayout = value; mapsToolbarView.RebuildFilters(); OnPropertyChanged(); } }
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

        // ========================================================
        //                     COMMANDS
        // ========================================================
        public ICommand ToolbarControlCommand { get; set; }
        public ICommand MasterToolbarCommand { get; set; }
        public ICommand SwapZOrderCommand { get; set; }
        public ICommand CursorCommand { get; set; }
        public ICommand BrightnessCommand { get; set; }
        public ICommand MapsCommand { get; set; }
        public ICommand ToggleTdmCommand { get; set; }
        public ICommand SwitchProfileCommand { get; set; }
        public ICommand GeneralSettingsCommand { get; set; }

        // ========================================================
        //                     ACTIONS
        // ========================================================

        public event Action? SwitchProfileAction;
        public event Action? GeneralSettingsAction;

        // ========================================================
        //                 CONSTRUCTOR
        // ========================================================

        public EramViewModel(Artcc artcc, Profile profile)
        {
            this.artcc = artcc;
            this.profile = profile;

            RadarViewModel = new RadarViewModel(this);

            SetTitle(this.artcc.id, string.Empty);
            StartZuluTimer();
            InitializeCommands();
            InitializeProfile();
            InitializeToolbarViews();
            InitializeMasterToolbar();
            InitializeZoom();
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

        // ========================================================
        //             PRIVATE METHODS
        // =====S===================================================

        private void SetTitle(string artcc, string sector)
        {
            if (sector == string.Empty)
            {
                Title = $"vFalcon : {artcc}";
            }
            else
            {
                Title = $"vFalcon : {artcc} : {sector}";
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
            ZuluTime = DateTime.UtcNow.ToString("HHmm ss");
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
            ToggleTdmCommand = new RelayCommand(() => { TdmActive = !TdmActive; RadarViewModel.Redraw(); });
            SwitchProfileCommand = new RelayCommand(OnSwitchProfileCommand);
            GeneralSettingsCommand = new RelayCommand(OnGeneralSettingsCommand);
        }

        private async Task InitializeProfile()
        {
            ActiveGeoMap = (string)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["ActiveGeoMap"] ?? null;
            LoadMapFilters();
            LoadBrightness();
            LoadTimeSettings();
            LoadToolbarControlMenu();
            LoadGeoMaps();
            await LoadVideoMaps();
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
                Logger.Debug("Maps Open", "setting master to new content");
                mapsToolbarView = new MapsToolbarView(this);
                MasterToolbarContent = mapsToolbarView;
                ActiveToolbar = mapsToolbarView;
            }
            else
            {
                Logger.Debug("Maps closed", "creating maps toolbar");
                mapsToolbarView = new MapsToolbarView(this);
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
            GeoMapStatus = "GEOMAPS UNAVAILABLE";
            FacilityFeatures = await GeoMap.LoadFacilityFeatures(artcc, ActiveVideoMapIds);
            GeoMapStatus = "GEOMAPS AVAILABLE";
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                GeoMapStatus = string.Empty;
            });
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
