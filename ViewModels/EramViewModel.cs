using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Services.Interfaces;
using vFalcon.Views;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace vFalcon.ViewModels
{
    public class EramViewModel : ViewModelBase
    {
        // Fields
        public Artcc artcc;
        public Profile profile;
        private MasterToolbarView masterToolbarView;
        private CursorToolbarView cursorToolbarView;
        private BrightnessToolbarView brightnessToolbarView;
        private MapsToolbarView mapsToolbarView;

        private double timeBorderLeft;
        private double timeBorderTop = double.NaN;
        private double timeBorderRight = double.NaN;
        private double timeBorderBottom = double.NaN;

        private double toolbarControlLeft;
        private double toolbarControlTop;
        private double toolbarControlRight;
        private double toolbarControlBottom;

        private object _masterToolbarContent;
        private ToolbarControlView _toolbarContent;

        private int _toolbarRegionZIndex = 1;
        private int _canvasZIndex = 2;
        private string masterRaiseLower;

        private Brush background;
        private int backgroundValue;
        private int brightnessValue;

        private bool cursorToolbarOpen = false;
        private bool brightnessToolbarOpen = false;
        private bool mapsToolbarOpen = false;

        private string mapsLabelLine1;
        private string mapsLabelLine2;


        //Default margins
        private Thickness timeMargin = new Thickness(20, 110, 0, 0);
        private Thickness toolbarControlMargin = new Thickness(90, 71, 0, 0);

        // Commands
        public ICommand ToolbarControlCommand { get; set; }
        public ICommand MasterToolbarCommand { get; set; }
        public ICommand SwapZOrderCommand { get; set; }
        
        //Master Toolbar
        public ICommand CursorCommand { get; set; }
        public ICommand BrightnessCommand { get; set; }

        public ICommand MapsCommand { get; set; }

        // Properties
        // Toolbar Control Location
        public double ToolbarControlLeft { get => toolbarControlLeft; set { toolbarControlLeft = value; OnPropertyChanged(); } }
        public double ToolbarControlRight { get => toolbarControlRight; set { toolbarControlRight = value; OnPropertyChanged(); } }
        public double ToolbarControlTop { get => toolbarControlTop; set { toolbarControlTop = value; OnPropertyChanged(); } }
        public double ToolbarControlBottom { get => toolbarControlBottom; set { toolbarControlBottom = value; OnPropertyChanged(); } }

        // Time Location
        public double TimeBorderLeft { get => timeBorderLeft; set { timeBorderLeft = value; OnPropertyChanged(); } }
        public double TimeBorderTop { get => timeBorderTop; set { timeBorderTop = value; OnPropertyChanged(); } }
        public double TimeBorderRight { get => timeBorderRight; set { timeBorderRight = value; OnPropertyChanged(); } }
        public double TimeBorderBottom { get => timeBorderBottom; set { timeBorderBottom = value; OnPropertyChanged(); } }

        // Master Toolbar Conent and Toolbar Control Content
        public object MasterToolbarContent { get => _masterToolbarContent; set { _masterToolbarContent = value; OnPropertyChanged(); } }
        public ToolbarControlView ToolbarControlContent { get => _toolbarContent; set { _toolbarContent = value; OnPropertyChanged(); } }

        // Default Margins
        public Thickness TimeMargin { get => timeMargin; set { timeMargin = value; OnPropertyChanged(); } }
        public Thickness ToolbarControlMargin { get => toolbarControlMargin; set { toolbarControlMargin = value; OnPropertyChanged(); } }


        // Toolbar Control Menu
        public int ToolbarRegionZIndex { get => _toolbarRegionZIndex; set { _toolbarRegionZIndex = value; OnPropertyChanged(); } }
        public int CanvasZIndex { get => _canvasZIndex; set { _canvasZIndex = value; OnPropertyChanged(); } }

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

        // Cursor Toolbar Menu
        public int CursorSize
        {
            get => (int)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"];
            set
            {
                if ((int)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"] != value)
                {
                    profile.DisplayWindowSettings[0]["DisplaySettings"][0]["CursorSize"] = value;
                    OnPropertyChanged();
                }
            }
        }

        // Brightness Toolbar Menu
        public Brush Background
        {
            get => background;
            set
            {
                if (background != value)
                {
                    background = value;
                    OnPropertyChanged();
                }
            }
        }
        public int BackgroundValue
        {
            get => backgroundValue;
            set
            {
                backgroundValue = value;
                OnPropertyChanged();
            }
        }

        public int BrightnessValue
        {
            get => brightnessValue;
            set
            {
                brightnessValue = value;
                OnPropertyChanged();
            }
        }

        // Maps Toolbar Menu
        public string ActiveGeoMap
        {
            get => (string)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"]["ACtiveGeoMap"];
            set
            {
                profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Bcgs"]["ACtiveGeoMap"] = value;
                OnPropertyChanged();
            }
        }

        public JArray ArtccGeoMaps
        {
            get => (JArray)artcc.facility["eramConfiguration"]["geoMaps"];
            set
            {
                artcc.facility["eramConfiguration"]["geoMaps"] = value;
                OnPropertyChanged();
            }
        }

        public string MapsLabelLine1
        {
            get => mapsLabelLine1;
            set
            {
                mapsLabelLine1 = value;
                OnPropertyChanged();
            }
        }

        public string MapsLabelLine2
        {
            get => mapsLabelLine2;
            set
            {
                mapsLabelLine2 = value;
                OnPropertyChanged();
            }
        }

        public void UpdateBackground()
        {
            double brightnessFactor = 0.65 + 0.35 * (BrightnessValue / 100.0); // min factor of 0.65, max factor of 1 (0.65+0.35)
            double baseBlue = ((BackgroundValue / 60.0) * 127) * brightnessFactor;
            byte blue = (byte)Math.Max(Math.Min(baseBlue, 255), 0);
            Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, blue));
        }

        public RadarViewModel RadarViewModel { get; set; }
        public HashSet<int> ActiveFilters { get; } = new HashSet<int>();

        // Constructor
        public EramViewModel(Artcc artcc, Profile profile)
        {
            this.artcc = artcc;
            this.profile = profile;

            RadarViewModel = new RadarViewModel(this);

            InitializeCommands();
            InitializeProfile();
            InitializeToolbarViews();
            InitializeMasterToolbar();
        }

        // Public Methods
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
            if (!IsMasterToolbarOpen)
            {
                IsMasterToolbarOpen = !IsMasterToolbarOpen;
                MasterToolbarContent = masterToolbarView;
            }
            else
            {
                IsMasterToolbarOpen = !IsMasterToolbarOpen;
                MasterToolbarContent = null;
            }
        }

        public void SwapZOrder()
        {
            if (IsRaiseMasterToolbar)
                SetZOrderLowered();
            else
                SetZOrderRaised();

            OnPropertyChanged(nameof(IsRaiseMasterToolbar));
        }

        public void OnCursorCommand()
        {
            if (!cursorToolbarOpen)
            {
                MasterToolbarContent = cursorToolbarView;
                cursorToolbarOpen = true;
            }
            else
            {
                MasterToolbarContent = masterToolbarView;
                cursorToolbarOpen = false;
            }
        }

        public void OnBrightnessCommand()
        {
            if (!brightnessToolbarOpen)
            {
                MasterToolbarContent = brightnessToolbarView;
                brightnessToolbarOpen = true;
            }
            else
            {
                MasterToolbarContent = masterToolbarView;
                brightnessToolbarOpen = false;
            }
        }

        public void OnMapsCommand()
        {
            if (!mapsToolbarOpen)
            {
                MasterToolbarContent = mapsToolbarView;
                mapsToolbarOpen = true;
            }
            else
            {
                MasterToolbarContent = masterToolbarView;
                mapsToolbarOpen = false;
            }
        }

        // Private Methods
        private void InitializeCommands()
        {
            MasterToolbarCommand = new RelayCommand(OnMasterToolbar);
            SwapZOrderCommand = new RelayCommand(SwapZOrder);
            CursorCommand = new RelayCommand(OnCursorCommand);
            BrightnessCommand = new RelayCommand(OnBrightnessCommand);
            MapsCommand = new RelayCommand(OnMapsCommand);
        }

        private void InitializeProfile()
        {
            LoadGeoMaps();
            LoadVideoMaps();
            LoadBrightness();
            LoadTimeSettings();
            LoadToolbarControlMenu();
        }


        private void InitializeMasterToolbar()
        {
            ToolbarControlContent = new ToolbarControlView(this);
            MasterRaiseLower = IsRaiseMasterToolbar ? "LOWER" : "RAISE";
            IsMasterToolbarOpen = !IsMasterToolbarOpen;
            OnMasterToolbar();

            if (IsRaiseMasterToolbar)
                SetZOrderRaised();
            else
                SetZOrderLowered();
        }

        private void InitializeToolbarViews()
        {
            masterToolbarView = new MasterToolbarView(this);
            cursorToolbarView = new CursorToolbarView(this);
            brightnessToolbarView = new BrightnessToolbarView(this);
            mapsToolbarView = new MapsToolbarView(this);
        }

        private void LoadGeoMaps()
        {
            ActiveGeoMap = (string)profile.DisplayWindowSettings[0]["DisplaySettings"][0]["ActiveGeoMap"] ?? null;
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

        public JArray ActiveVideoMapIds;

        public Dictionary<int, List<ProcessedFeature>> FacilityFeatures { get; } = new Dictionary<int, List<ProcessedFeature>>();

        private async Task LoadVideoMaps()
        {
            string sourcePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CRC", $"VideoMaps/{artcc.id}");

            var matchedFiles = ActiveVideoMapIds
                .Select(id => Path.Combine(sourcePath, id + ".geojson"))
                .Where(File.Exists)
                .ToList();

            foreach (var file in matchedFiles)
            {
                var json = await File.ReadAllTextAsync(file);
                JObject geoJson = JsonConvert.DeserializeObject<JObject>(json);

                var defaults = ExtractLastIsDefaults(geoJson);
                if (geoJson == null) continue;
                foreach (var feature in geoJson["features"])
                {
                    var properties = (JObject)feature["properties"];
                    if (IsDefaultsFeature(properties)) continue;

                    string geometryType = (string)feature["geometry"]["type"];
                    var finalProperties = MergeProperties(properties, defaults, geometryType);
                    var filters = finalProperties.ContainsKey("filters")
                        ? ((JArray)finalProperties["filters"]).Select(f => (int)f).ToList()
                        : new List<int>();

                    foreach (var filter in filters)
                    {
                        if (!FacilityFeatures.ContainsKey(filter))
                            FacilityFeatures[filter] = new List<ProcessedFeature>();

                        FacilityFeatures[filter].Add(new ProcessedFeature
                        {
                            GeometryType = ResolveFeatureType(geometryType, properties),
                            Geometry = feature["geometry"],
                            AppliedAttributes = finalProperties
                        });
                    }
                }
            }

            Logger.Alert("EramViewModel.LoadVideoMaps", "COMPLETED");
        }

        private string ResolveFeatureType(string geometryType, JObject props)
        {
            if (geometryType == "LineString" || geometryType == "MultiLineString")
                return "Line";
            if (geometryType == "Point" && props.ContainsKey("text"))
                return "Text";
            if (geometryType == "Point")
                return "Symbol";
            return geometryType; // fallback
        }

        private bool IsDefaultsFeature(JObject props)
        {
            if (props == null) return false;

            return props.Value<bool?>("isLineDefaults") == true ||
                   props.Value<bool?>("isSymbolDefaults") == true ||
                   props.Value<bool?>("isTextDefaults") == true;
        }

        private Dictionary<string, object> MergeProperties(JObject featureProps, FeatureDefaults defaults, string geometryType)
        {
            var merged = new Dictionary<string, object>();

            if (featureProps == null)
            {
                Dictionary<string, object> typeDefaultsOnly = geometryType switch
                {
                    "LineString" or "MultiLineString" => defaults.LineDefaults,
                    "Point" => defaults.SymbolDefaults,
                    _ => new Dictionary<string, object>()
                };

                foreach (var kv in typeDefaultsOnly)
                    merged[kv.Key] = kv.Value;

                ApplyCrcAutoDefaults(merged, geometryType);
                return merged;
            }

            Dictionary<string, object> typeDefaults = geometryType switch
            {
                "LineString" or "MultiLineString" => defaults.LineDefaults,
                "Point" => featureProps.ContainsKey("text")
                              ? defaults.TextDefaults
                              : defaults.SymbolDefaults,
                _ => new Dictionary<string, object>()
            };

            foreach (var kv in typeDefaults)
                merged[kv.Key] = kv.Value;

            foreach (var kv in featureProps)
                merged[kv.Key] = kv.Value;

            ApplyCrcAutoDefaults(merged, geometryType);

            return merged;
        }

        private void ApplyCrcAutoDefaults(Dictionary<string, object> props, string featureType)
        {
            switch (featureType)
            {
                case "Line":
                    if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                    if (!props.ContainsKey("style")) props["style"] = "solid";
                    if (!props.ContainsKey("thickness")) props["thickness"] = 1;
                    break;

                case "Symbol":
                    if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                    if (!props.ContainsKey("style")) props["style"] = "vor";
                    if (!props.ContainsKey("size")) props["size"] = 1;
                    break;

                case "Text":
                    if (!props.ContainsKey("bcg")) props["bcg"] = 1;
                    if (!props.ContainsKey("size")) props["size"] = 1;
                    if (!props.ContainsKey("underline")) props["underline"] = false;
                    if (!props.ContainsKey("xOffset")) props["xOffset"] = 0;
                    if (!props.ContainsKey("yOffset")) props["yOffset"] = 0;
                    if (!props.ContainsKey("opaque")) props["opaque"] = false;
                    break;
            }
        }

        private FeatureDefaults ExtractLastIsDefaults(JObject geojson)
        {
            var defaults = new FeatureDefaults
            {
                LineDefaults = new Dictionary<string, object>(),
                SymbolDefaults = new Dictionary<string, object>(),
                TextDefaults = new Dictionary<string, object>()
            };

            if (geojson?["features"] is not JArray featuresArray)
                return defaults;

            foreach (var feature in featuresArray)
            {
                var props = feature["properties"] as JObject;
                if (props == null) continue;

                if (props.Value<bool?>("isLineDefaults") == true)
                    defaults.LineDefaults = props.ToObject<Dictionary<string, object>>();

                if (props.Value<bool?>("isSymbolDefaults") == true)
                    defaults.SymbolDefaults = props.ToObject<Dictionary<string, object>>();

                if (props.Value<bool?>("isTextDefaults") == true)
                    defaults.TextDefaults = props.ToObject<Dictionary<string, object>>();
            }

            return defaults;
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

            double[] parts = ((string)timeViewSettings["Location"]["Location"])
                .Split(',')
                .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                .ToArray();

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
