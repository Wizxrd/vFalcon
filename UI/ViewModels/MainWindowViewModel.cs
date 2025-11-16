using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using vFalcon.Engines;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Nasr;
using vFalcon.Renderables.Interfaces;
using vFalcon.Services;
using vFalcon.UI.ViewModels.Controls;
using vFalcon.UI.Views.Controls;
using vFalcon.Utils;
using Size = System.Drawing.Size;
namespace vFalcon.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DisplayControlView DisplayControlView { get; set; }
    private string title = string.Empty;
    private string lastDataFeedUpdate = "00:00:00z";
    private string recordingDuration = string.Empty;
    private Stopwatch recordingStopwatch;
    private string displayStatus = string.Empty;
    private string mouseCoordinates = string.Empty;
    private Visibility displayVisibility = Visibility.Collapsed;
    private Visibility replayControlVisibility = Visibility.Collapsed;

    public Popup PilotContextPopup { get; set; }
    public PilotContextView PilotContextView { get; set; }
    public PilotContextViewModel PilotContextViewModel { get; set; }
    public SkiaEngine GraphicsEngine { get; set; }
    public RouteService RouteService { get; set; } = new(null);
    public ProfileService ProfileService = new();
    public DisplayState DisplayState { get; set; }
    public PilotService PilotService { get; set; }
    public PlaybackService PlaybackService { get; set; } = new();
    public DatablockService DatablockService { get; set; }
    public RecordingService RecordingService { get; set; }
    public FindService FindService { get; set; } = new();
    public ScheduledFunction RecordingDurationScheduler { get; set; }
    public SKPoint MousePosition { get; set; }
    public bool DisplayMouseCoordinates { get; set; } = false;
    public bool IsRecording { get; set; } = false;
    public bool IsPlayback { get; set; } = false;
 
    public string Title
    {
        get => title;
        set
        {
            title = value;
            OnPropertyChanged();
        }
    }
    public string LastDataFeedUpdate
    {
        get => lastDataFeedUpdate;
        set
        {
            lastDataFeedUpdate = value;
            OnPropertyChanged();
        }
    }
    public string RecordingDuration
    {
        get => recordingDuration;
        set
        {
            recordingDuration = value;
            OnPropertyChanged();
        }
    }
    public string DisplayStatus
    {
        get => displayStatus;
        set
        {
            displayStatus = value;
            OnPropertyChanged();
        }
    }
    public string MouseCoordinates
    {
        get => mouseCoordinates;
        set
        {
            mouseCoordinates = value;
            OnPropertyChanged();
        }
    }
    public Visibility DisplayVisibility
    {
        get => displayVisibility;
        set
        {
            displayVisibility = value;
            OnPropertyChanged();
        }
    }
    public Visibility ReplayControlVisibility
    {
        get => replayControlVisibility;
        set
        {
            replayControlVisibility = value;
            OnPropertyChanged();
        }
    }

    public HashSet<string> ActiveMaps { get; set; } = new();
    public Dictionary<string, HashSet<string>> ActiveEramMaps { get; set; } = new();
    public Dictionary<string, List<ProcessedFeature>> StarsFeatures { get; set; } = new();
    public Dictionary<string, List<ProcessedFeature>> EramFeatures { get; set; } = new();
    public Dictionary<string, Dictionary<string, List<ProcessedFeature>>> EramFeaturesCombined { get; set; } = new();
    public List<ProcessedFeature> RenderableFeatures { get; set; } = new();
    public List<double> ZoomLevels { get; set; } = new();
    public List<double> ScaleMap { get; set; } = new();
    public List<Coordinate> SurveillanceAoi { get; set; } = new();

    public ICommand ClearCommand { get; set; }
    public ICommand ToggleFullscreenCommand { get; set; }
    public ICommand ToggleTitleBarCommand { get; set; }
    public ICommand ToggleResizeBorderCommand { get; set; }
    public ICommand ToggleRecordingCommand { get; set; }
    public ICommand CaptureScreenCommand { get; set; }
    public ICommand LoadRecordingCommand { get; set; }
    public ICommand ExitRecordingCommand { get; set; }
    public ICommand ToggleTopDownCommand { get; set; }
    public ICommand SaveProfileCommand { get; set; }
    public ICommand SaveProfileAsCommand { get; set; }
    public ICommand SwitchProfileCommand { get; set; }
    public ICommand OpenGeneralSettingsCommand { get; set; }
    public ICommand OpenAppearanceSettingsCommand { get; set; }
    public ICommand OpenMapsCommand { get; set; }
    public ICommand OpenPositionsCommand { get; set; }
    public ICommand RewindCommand { get; set; }
    public ICommand FastForwardCommand { get; set; }
    public ICommand PlayPauseCommand { get; set; }
    public ICommand IncreaseVelocityVectorCommand { get; set; }
    public ICommand DecreaseVelocityVectorCommand { get; set; }

    public MainWindowViewModel(DisplayControlView displayControlView)
    {
        DisplayControlView = displayControlView;
        PilotContextPopup = App.MainWindowView.PilotContextPopup;
        PilotContextView = App.MainWindowView.PilotContextView;
        PilotContextViewModel = PilotContextView.DataContext as PilotContextViewModel;
        GraphicsEngine = DisplayControlView.GraphicsEngine;
        GraphicsEngine.SizeChanged += OnSizeChanged;
        App.MainWindowView.KeyDown += OnKeyDown;
        App.MainWindowView.KeyUp += OnKeyUp;
        GraphicsEngine.MouseDown += OnMouseDown;
        GraphicsEngine.MouseUp += OnMouseUp;
        GraphicsEngine.MouseMove += OnMouseMove;
        GraphicsEngine.MouseWheel += OnMouseWheel;
        GraphicsEngine.PaintSurface += PaintSurface;

        DisplayState = new();
        PilotService = new();
        DatablockService = new();
        RecordingService = new();
        PilotService.DisplayState = DisplayState;

        ClearCommand = new RelayCommand(OnClearCommand);
        ToggleFullscreenCommand = new RelayCommand(OnToggleFullscreenCommand);
        ToggleTitleBarCommand = new RelayCommand(OnToggleTitleBarCommand);
        ToggleResizeBorderCommand = new RelayCommand(OnToggleResizeBorderCommand);
        ToggleRecordingCommand = new RelayCommand(OnToggleRecordingCommand);
        CaptureScreenCommand = new RelayCommand(OnCaptureScreenCommand);
        LoadRecordingCommand = new RelayCommand(OnLoadRecordingCommand);
        ExitRecordingCommand = new RelayCommand(OnExitRecordingCommand);
        ToggleTopDownCommand = new RelayCommand(OnToggleTopDownCommand);
        SaveProfileCommand = new RelayCommand(OnSaveProfileCommand);
        SaveProfileAsCommand = new RelayCommand(OnSaveProfileAsCommand);
        SwitchProfileCommand = new RelayCommand(OnSwitchProfileCommand);
        OpenGeneralSettingsCommand = new RelayCommand(OnOpenGeneralSettingsCommand);
        OpenAppearanceSettingsCommand = new RelayCommand(OnOpenAppearanceSettingsCommand);
        OpenMapsCommand = new RelayCommand(OnOpenMapsCommand);
        OpenPositionsCommand = new RelayCommand(OnOpenPositionsCommand);
        RewindCommand = new RelayCommand(OnRewindCommand);
        FastForwardCommand = new RelayCommand(OnFastForwardCommand);
        PlayPauseCommand = new RelayCommand(OnPlayPauseCommand);
        IncreaseVelocityVectorCommand = new RelayCommand(() => PilotService.IncreaseVelocityVector());
        DecreaseVelocityVectorCommand = new RelayCommand(() => PilotService.DecreaseVelocityVector());

        Initialize();

        App.MainWindowViewModel = this;
    }

    private async void Initialize()
    {
        SetTitle();
        SetBackground();
        SetSurveillanceAoi();
        LoadAppearanceSettings();
        SetDisplayReady(false);
        await SetFeatures();
        SetDisplayState();
        SetActiveMaps();
        StartServices();
        SetDisplayReady(true);
    }

    public void SetDisplayState()
    {
        if (App.Profile.DisplaySettings.Center == null)
        {
            double lat = (double)App.Artcc.visibilityCenters[0]["lat"];
            double lon = (double)App.Artcc.visibilityCenters[0]["lon"];
            DisplayState.Center.Lat = lat;
            DisplayState.Center.Lon = lon;
        }
        else
        {
            DisplayState.Center = App.Profile.DisplaySettings.Center;
        }
        ZoomLevels = Zoom.BuildLevels();
        ScaleMap = Zoom.BuildScale(DisplayState, ZoomLevels);
        DisplayState.Scale = ScaleMap[App.Profile.DisplaySettings.ZoomIndex];
        DisplayState.PanOffset = CenterAtCoordinates(DisplayState.Width, DisplayState.Height, DisplayState.Scale, DisplayState.PanOffset, DisplayState.Center);
    }

    public void StartServices()
    {
        PilotService.Start();
        DatablockService.Start();
    }

    public void SetActiveMaps()
    {
        foreach (string filter in App.Profile.ActiveEramFilters)
        {
            ActiveEramMaps[App.Profile.MapSettings.GeoMap].Add(filter);
        }
        foreach (string videoMap in App.Profile.ActiveStarsVideoMaps)
        {
            ActiveMaps.Add(videoMap);
        }
        SetRenderableFeatures();
        GraphicsEngine.RequestRender();
    }

    public void LoadAppearanceSettings()
    {
        Colors.EramFullDatablock = SkiaEngine.ScaleColor(Colors.Yellow, App.Profile.AppearanceSettings.FullDatablockBrightness);
        Colors.StarsFullDatablock = SkiaEngine.ScaleColor(Colors.White, App.Profile.AppearanceSettings.FullDatablockBrightness);
        Colors.EramLimitedDatablock = SkiaEngine.ScaleColor(Colors.Yellow, App.Profile.AppearanceSettings.LimitedDatablockBrightness);
        Colors.StarsLimitedDatablock = SkiaEngine.ScaleColor(Colors.LimeGreen, App.Profile.AppearanceSettings.LimitedDatablockBrightness);
        Colors.EramFullDatablockHistory = SkiaEngine.ScaleColor(Colors.Yellow, App.Profile.AppearanceSettings.HistoryBrightness);
        Colors.EramLimitedDatablockHistory = SkiaEngine.ScaleColor(Colors.Yellow, App.Profile.AppearanceSettings.HistoryBrightness);
    }

    public void SetTitle()
    {
        Title = $"vFalcon : Live : {App.Profile.ArtccId}";
        if (IsPlayback)
        {
            Title = $"vFalcon : Replay : {App.Profile.ArtccId}";
        }
    }

    public void SetBackground()
    {
        GraphicsEngine.BackgroundValue = App.Profile.AppearanceSettings.Background;
        GraphicsEngine.BacklightValue = App.Profile.AppearanceSettings.Backlight;
        GraphicsEngine.ScaleBackgroundByBacklight();
    }

    public void SetSurveillanceAoi()
    {
        string json = File.ReadAllText(Path.Combine(PathFinder.GetAppDirectory(), "SurveillanceAois.geojson"));
        Surveillance surveillance = JsonConvert.DeserializeObject<Surveillance>(json);
        foreach (JObject feature in surveillance.features)
        {
            var id = ((JObject)feature["properties"])?.GetValue("id", StringComparison.OrdinalIgnoreCase)?.ToString();
            if (string.Equals(id, App.Profile.ArtccId))
            {
                JObject geometry = (JObject)feature["geometry"];
                JArray coordinates = (JArray)geometry["coordinates"];
                foreach (JArray coordinate in coordinates[0])
                {
                    double lat = (double)coordinate[1];
                    double lon = (double)coordinate[0];
                    SurveillanceAoi.Add(new Coordinate
                    {
                        Lat = (double)coordinate[1],
                        Lon = (double)coordinate[0],
                    });
                }
                return;
            }
        }
    }

    public void SetDisplayReady(bool ready)
    {
        if (ready)
        {
            DisplayStatus = string.Empty;
            DisplayState.IsReady = true;
            DisplayVisibility = Visibility.Visible;
        }
        else
        {
            DisplayStatus = "Display not ready";
            DisplayState.IsReady = false;
            DisplayVisibility = Visibility.Collapsed;
        }
    }

    public async Task SetFeatures()
    {
        EramFeatures.Clear();
        EramFeaturesCombined.Clear();
        ActiveEramMaps.Clear();
        foreach (JObject geoMap in Models.VideoMap.GetGeoMaps()) ActiveEramMaps[(string)geoMap["name"]] = new HashSet<string>();
        if (App.Profile.MapSettings.GeoMap == string.Empty) App.Profile.MapSettings.GeoMap = (string)App.Artcc.facility["eramConfiguration"]["geoMaps"][0]["name"];
        JObject defaultStarsFacility = (JObject)App.Artcc.facility["childFacilities"][0];
        if (App.Profile.MapSettings.Facility == string.Empty) App.Profile.MapSettings.Facility = $"{defaultStarsFacility["id"]} - {defaultStarsFacility["name"]}";
        if (App.Profile.GeneralSettings.VideoMapPreProcess)
        {
            Logger.Debug("InitFeatures", "Video Map Pre-Processing Enabled");
            StarsFeatures = await GeoJson.GetAllStarsFeatures();
            EramFeaturesCombined = await GeoJson.GetAllEramFeatures();
        }
        else
        {
            EramFeatures = await GeoJson.GetEramFacilityFeatures(App.Artcc, JArray.FromObject(VideoMap.GetMapsetVideoMapIds(App.Profile.MapSettings.GeoMap)));
            StarsFeatures = await GeoJson.GetStarsFacilityFeatures(App.Artcc, VideoMap.GetFaciltiyVideoMaps(App.Profile.MapSettings.Facility.Substring(0, 3)));
        }
        SetRenderableFeatures();
    }

    public async Task ReloadFeatures()
    {
        SetDisplayReady(false);
        await SetFeatures();
        SetDisplayReady(true);
        SetRenderableFeatures();
    }

    public async void ReloadEramFeatures(bool reloadEramFeatures = false)
    {;
        if (!App.Profile.GeneralSettings.VideoMapPreProcess && reloadEramFeatures)
        {
            EramFeatures.Clear();
            DisplayState.IsReady = false;
            DisplayVisibility = Visibility.Collapsed;
            DisplayStatus = "Display not ready";
            EramFeatures = await GeoJson.GetEramFacilityFeatures(App.Artcc, JArray.FromObject(VideoMap.GetMapsetVideoMapIds(App.Profile.MapSettings.GeoMap)));
            DisplayStatus = string.Empty;
            DisplayState.IsReady = true;
            DisplayVisibility = Visibility.Visible;
        }
        else if (App.Profile.GeneralSettings.VideoMapPreProcess)
        {
            EramFeatures = EramFeaturesCombined[App.Profile.MapSettings.GeoMap];
        }
        SetRenderableFeatures();
        GraphicsEngine.RequestRender();
    }

    public async void ReloadStarsFeatures(string facilityId)
    {
        if (!App.Profile.GeneralSettings.VideoMapPreProcess)
        {
            StarsFeatures.Clear();
            DisplayState.IsReady = false;
            DisplayVisibility = Visibility.Collapsed;
            DisplayStatus = "Display not ready";
            StarsFeatures = await GeoJson.GetStarsFacilityFeatures(App.Artcc, VideoMap.GetFaciltiyVideoMaps(facilityId));
            DisplayStatus = string.Empty;
            DisplayState.IsReady = true;
            DisplayVisibility = Visibility.Visible;
        }
        SetRenderableFeatures();
    }

    public void SetRenderableFeatures()
    {
        RenderableFeatures = new();
        AddEramRenderableFeatures();
        AddStarsRenderableFeatures();
    }

    public async Task UpdateRecordingDuration()
    {
        TimeSpan elapsed = recordingStopwatch.Elapsed;
        RecordingDuration = $"Recording: {elapsed:hh\\:mm\\:ss}";
    }

    public void StartRecording()
    {
        if (IsPlayback)
        {
            Message.Confirm("Cannot record when in playback mode");
            return;
        }
        IsRecording = true;
        recordingStopwatch = new();
        RecordingDurationScheduler = new(UpdateRecordingDuration, 1, true);
        recordingStopwatch.Start();
        RecordingDurationScheduler.Start();
        RecordingService.Start();
    }

    public void StopRecording()
    {
        IsRecording = false;
        RecordingDuration = string.Empty;
        recordingStopwatch.Stop();
        RecordingDurationScheduler.Stop();
        RecordingService.Stop();
    }

    public Coordinate GetCursorCoordinate(SKPoint mousePosition)
    {
        return ScreenMap.ScreenToCoordinate(DisplayState.Size, DisplayState.Scale, DisplayState.PanOffset, mousePosition);
    }

    public SKPoint CenterAtCoordinates(int width, int height, double scale, SKPoint panOffset, Coordinate coordinate)
    {
        var screenPoint = ScreenMap.CoordinateToScreen(width, height, scale, panOffset, coordinate.Lat, coordinate.Lon);
        float shiftX = (width / 2f) - screenPoint.X;
        float shiftY = (height / 2f) - screenPoint.Y;
        panOffset.X += shiftX;
        panOffset.Y += shiftY;
        return panOffset;
    }

    private void AddStarsRenderableFeatures()
    {
        if (StarsFeatures != null)
        {
            foreach (var kvp in StarsFeatures)
            {
                if (!ActiveMaps.Contains(kvp.Key)) continue;
                RenderableFeatures.AddRange(kvp.Value);
            }
        }
    }

    private void AddEramRenderableFeatures()
    {
        if (EramFeatures != null)
        {
            foreach (var kvp in EramFeatures)
            {
                if (!ActiveEramMaps[App.Profile.MapSettings.GeoMap].Contains(kvp.Key)) continue;
                var filtered = kvp.Value.Where(f =>
                {
                    if (f.AppliedAttributes.TryGetValue("tdmOnly", out var tdmVal))
                    {
                        bool isTdmOnly = Convert.ToBoolean(tdmVal);
                        if (isTdmOnly && !App.Profile.GeneralSettings.TopDown)
                            return false;
                    }
                    return true;
                });
                RenderableFeatures.AddRange(filtered);
            }
        }
    }

    private void OpenPilotContextView(object sender, SKPoint point)
    {
        PilotContextPopup.IsOpen = false;
        Pilot? clickedOnPilot = PilotService.IsPilotClickedOn(point);
        if (clickedOnPilot != null)
        {
            string flightRules = (string)clickedOnPilot.FlightPlan?["flight_rules"] ?? string.Empty;
            string flightPlan = $"{clickedOnPilot.FlightPlan?["departure"]} {clickedOnPilot.FlightPlan?["route"]} {clickedOnPilot.FlightPlan?["arrival"]}";
            flightPlan = flightPlan.Replace("DCT ", "");
            if (flightRules == "I") flightRules = "IFR";
            else if (flightRules == "V") flightRules = "VFR";
            string requestedAltitude = (string)clickedOnPilot.FlightPlan?["altitude"];
            if (requestedAltitude == null) requestedAltitude = "VFR";
            if (requestedAltitude.Contains("VFR")) flightRules = "VFR";
            PilotContextPopup.HorizontalOffset = point.X;
            PilotContextPopup.VerticalOffset = point.Y + 10;
            PilotContextPopup.PlacementTarget = (FrameworkElement)sender;
            PilotContextViewModel.Pilot = clickedOnPilot;
            PilotContextViewModel.DatablockType = (int)clickedOnPilot.DatablockType;
            PilotContextViewModel.DriEnabled = clickedOnPilot.DriEnabled;
            PilotContextViewModel.DriSize = clickedOnPilot.DriSize;
            PilotContextViewModel.DatablockPosition = (int)clickedOnPilot.DatablockPosition;
            PilotContextViewModel.LeaderLineLength = clickedOnPilot.LeaderLineLength;
            PilotContextViewModel.Time = $"Time: {DateTime.Now:MM/dd/yy HH:mm:ss}";
            PilotContextViewModel.PilotCallsign = $"Callsign: {clickedOnPilot.Callsign}";
            PilotContextViewModel.CID = $"CID: {clickedOnPilot.CID}";
            PilotContextViewModel.Type = $"Type: {clickedOnPilot.FlightPlan?["aircraft_faa"] ?? string.Empty}";
            PilotContextViewModel.ReportedAltitude = $"Reported Altitude: {clickedOnPilot.Altitude}";
            PilotContextViewModel.RequestedAltitude = $"Requested Altitude: {requestedAltitude}";
            PilotContextViewModel.Heading = $"Course Mag. Heading: {clickedOnPilot.Heading}";
            PilotContextViewModel.Speed = $"Speed: {clickedOnPilot.GroundSpeed}";
            PilotContextViewModel.AssignedBeacon = $"Beacon: {clickedOnPilot.Transponder}";
            PilotContextViewModel.Frequency = $"Frequency: {clickedOnPilot.Frequency}";
            PilotContextViewModel.FlightRules = $"Flight Rules: {flightRules}";
            PilotContextViewModel.LatLon = $"Lat/Lon: {clickedOnPilot.Latitude:F2}/{clickedOnPilot.Longitude:F2}";
            PilotContextViewModel.FlightPlan = flightPlan;
            PilotContextPopup.IsOpen = true;
        }
    }

    private void OnSizeChanged(double width, double height)
    {
        PilotContextPopup.IsOpen = false;
        DisplayState.Width = (int)width;
        DisplayState.Height = (int)height;
        ScaleMap = Zoom.BuildScale(DisplayState, ZoomLevels);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;
        if (key == Key.LeftAlt || key == Key.RightAlt)
        {
            KeybindHeld.Alt = true;
            DisplayState.ZoomOnMouse = true;
        }
        if (key == Key.LeftCtrl || key == Key.RightCtrl)
        {
            KeybindHeld.Ctrl = true;
            DisplayState.DoubleZoom = true;
        }
        if (key == Key.LeftShift || key == Key.RightShift)
        {
            KeybindHeld.Shift = true;
        }
        if (key == Key.J)
        {
            KeybindHeld.J = true;
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;
        if (key == Key.LeftAlt || key == Key.RightAlt)
        {
            KeybindHeld.Alt = false;
            DisplayState.ZoomOnMouse = false;
        }
        if (key == Key.LeftCtrl || key == Key.RightCtrl)
        {
            KeybindHeld.Ctrl = false;
            DisplayState.DoubleZoom = false;
        }
        if (key == Key.LeftShift || key == Key.RightShift)
        {
            KeybindHeld.Shift = false;
        }
        if (key == Key.J)
        {
            KeybindHeld.J = false;
        }
    }

    private void OnMouseDown(object sender, SKPoint point, MouseButton button)
    {
        if (!DisplayState.IsReady) return;
        if (button == MouseButton.Right)
        {
            if (KeybindHeld.Ctrl)
            {
                Pilot? clickedOnPilot = PilotService.IsPilotClickedOn(point);
                if (clickedOnPilot != null)
                {
                    PilotService.CycleLeaderLineLength(clickedOnPilot);
                    GraphicsEngine.RequestRender();
                    return;
                }
            }
            OpenPilotContextView(sender, point);
            if (MousePosition == point) return;
            MousePosition = point;
            DisplayState.IsPanning = true;
            GraphicsEngine.StartPan();
        }
        if (button == MouseButton.Middle)
        {
            Pilot? clickedOnPilot = PilotService.IsPilotClickedOn(point);
            if (clickedOnPilot != null)
            {
                clickedOnPilot.ForcedFullDatablock = !clickedOnPilot.ForcedFullDatablock;
                clickedOnPilot.FullDatablockEnabled = !clickedOnPilot.FullDatablockEnabled;
                GraphicsEngine.RequestRender();
            }
        }
        if (button == MouseButton.Left)
        {
            Pilot? clickedOnPilot = PilotService.IsPilotClickedOn(point);
            if (clickedOnPilot == null) return;
            if (KeybindHeld.Ctrl)
            {
                PilotService.CycleDatablockPosition(clickedOnPilot);
            }
            if (KeybindHeld.Shift)
            {
                clickedOnPilot.DriEnabled = !clickedOnPilot.DriEnabled;
            }
            if (KeybindHeld.Alt)
            {
                clickedOnPilot.DwellEmphasisEnabled = !clickedOnPilot.DwellEmphasisEnabled;
            }
            if (KeybindHeld.J)
            {
                PilotService.CycleDriSize(clickedOnPilot);
            }
            GraphicsEngine.RequestRender();
        }
    }

    private void OnMouseMove(object sender, SKPoint point)
    {
        if (!DisplayState.IsReady) return;
        MouseCoordinates = string.Empty;
        if (DisplayMouseCoordinates)
        {
            Coordinate mouseCoordinates = GetCursorCoordinate(point);
            MouseCoordinates = $"{mouseCoordinates.Lat:F2}, {mouseCoordinates.Lon:F2}";
        }
        if (!DisplayState.IsPanning) return;
        PilotContextPopup.IsOpen = false;
        SKPoint delta = point - (SKPoint)MousePosition;
        DisplayState.PanOffset = new SKPoint(
            DisplayState.PanOffset.X + delta.X,
            DisplayState.PanOffset.Y + delta.Y
        );
        MousePosition = point;
        GraphicsEngine.RequestRender();
    }

    private void OnMouseUp(object sender, SKPoint point, MouseButton button)
    {
        if (!DisplayState.IsReady) return;
        if (button == MouseButton.Right)
        {
            Coordinate newCenter = ScreenMap.ScreenToCoordinate(new System.Drawing.Size(DisplayState.Width, DisplayState.Height), DisplayState.Scale, DisplayState.PanOffset, new SKPoint(DisplayState.Width / 2f, DisplayState.Height / 2f));
            DisplayState.Center.Lat = newCenter.Lat;
            DisplayState.Center.Lon = newCenter.Lon;
            App.Profile.DisplaySettings.Center = newCenter;
			MousePosition = new SKPoint();
            DisplayState.IsPanning = false;
            GraphicsEngine.StopPan();
        }
    }

    private void OnMouseWheel(object sender, SKPoint point, int delta)
    {
        if (!DisplayState.IsReady) return;
        PilotContextPopup.IsOpen = false;
        int direction = delta > 0 ? 1 : -1;
        int step = DisplayState.DoubleZoom ? 4 : 1;
        int newIndex = Math.Clamp(App.Profile.DisplaySettings.ZoomIndex + direction * step, 0, ZoomLevels.Count - 1);
        if (newIndex == App.Profile.DisplaySettings.ZoomIndex) return;

        SKPoint referencePoint = DisplayState.ZoomOnMouse ? point : new SKPoint(DisplayState.Width / 2f, DisplayState.Height / 2f);

        var before = new SKPoint(
            (referencePoint.X - DisplayState.PanOffset.X - DisplayState.Width / 2f) / (float)DisplayState.Scale,
            (referencePoint.Y - DisplayState.PanOffset.Y - DisplayState.Height / 2f) / (float)DisplayState.Scale
        );

        App.Profile.DisplaySettings.ZoomIndex = newIndex;

        DisplayState.Scale = ScaleMap[App.Profile.DisplaySettings.ZoomIndex];

        var after = new SKPoint(
            (referencePoint.X - DisplayState.PanOffset.X - DisplayState.Width / 2f) / (float)DisplayState.Scale,
            (referencePoint.Y - DisplayState.PanOffset.Y - DisplayState.Height / 2f) / (float)DisplayState.Scale
        );

        var diff = after - before;
        DisplayState.PanOffset = new SKPoint(
            DisplayState.PanOffset.X + diff.X * (float)DisplayState.Scale,
            DisplayState.PanOffset.Y + diff.Y * (float)DisplayState.Scale
        );
        if (!DisplayState.ZoomOnMouse && !DisplayState.IsPanning)
        {
            DisplayState.PanOffset = CenterAtCoordinates(DisplayState.Width, DisplayState.Height, DisplayState.Scale, DisplayState.PanOffset, DisplayState.Center);
        }
        else
        {
            var newCenter = ScreenMap.ScreenToCoordinate(
                new System.Drawing.Size(DisplayState.Width, DisplayState.Height),
                DisplayState.Scale,
                DisplayState.PanOffset,
                new SKPoint(DisplayState.Width / 2f, DisplayState.Height / 2f)
            );

            DisplayState.Center = new Coordinate { Lat = newCenter.Lat, Lon = newCenter.Lon };
            var pOffset = DisplayState.PanOffset;
            DisplayState.PanOffset = pOffset;
        }
        GraphicsEngine.RequestRender();
    }

    private void PaintSurface(SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear();
        SKImageInfo info = e.Info;
        DisplayState.Size = new Size(info.Width, info.Height);
        GraphicsEngine.Renderables.Clear();
        GraphicsEngine.RenderEngine.Canvas = canvas;
        if (GraphicsEngine.RenderEngine.Size != new Size(DisplayState.Width, DisplayState.Height)) GraphicsEngine.RenderEngine.Size = new Size(DisplayState.Width, DisplayState.Height);
        if (GraphicsEngine.RenderEngine.Scale != DisplayState.Scale) GraphicsEngine.RenderEngine.Scale = DisplayState.Scale;
        if (GraphicsEngine.RenderEngine.PanOffset != DisplayState.PanOffset) GraphicsEngine.RenderEngine.PanOffset = DisplayState.PanOffset;
        if (RenderableFeatures.Count > 0)
        {
            List<IRenderable> videoMapRenderables = Renderables.Renderables.FromFeatures(DisplayState, RenderableFeatures);
            GraphicsEngine.Renderables.AddRange(videoMapRenderables);
        }

        if (PilotService.Pilots.Count > 0)
        {
            List<IRenderable> pilotRenderables = Renderables.Renderables.FromPilots(DisplayState, PilotService.Pilots);
            GraphicsEngine.Renderables.AddRange(pilotRenderables);
        }

        if (FindService.ThingsToFind.Count > 0)
        {
            List<IRenderable> findRenderables = Renderables.Renderables.FromThingsToFind(DisplayState, FindService.ThingsToFind);
            GraphicsEngine.Renderables.AddRange(findRenderables);
        }

        GraphicsEngine.RenderEngine.UpdateRenderables(GraphicsEngine.Renderables);
        GraphicsEngine.RenderEngine.Render();
    }

    private void OnClearCommand()
    {
        List<Pilot> pilots = PilotService.Pilots.Values.ToList();
        if (pilots.Count < 1 || pilots == null) return;
        foreach (Pilot pilot in pilots)
        {
            if (pilot.DisplayFiledRoute) pilot.DisplayFiledRoute = false;
            if (pilot.DisplayFullRoute) pilot.DisplayFullRoute = false;
        }
        PilotContextPopup.IsOpen = false;
        FindService.ThingsToFind.Clear();
        GraphicsEngine.RequestRender();
    }

    public void OnToggleFullscreenCommand()
    {
        if (App.MainWindowView.WindowState == WindowState.Maximized)
        {
            App.MainWindowView.WindowStyle = WindowStyle.SingleBorderWindow;
            App.MainWindowView.ResizeMode = ResizeMode.CanResize;
            App.MainWindowView.WindowState = WindowState.Normal;
            App.Profile.WindowSettings.IsFullscreen = false;
        }
        else
        {
            App.MainWindowView.WindowStyle = WindowStyle.None;
            App.MainWindowView.ResizeMode = ResizeMode.NoResize;
            App.MainWindowView.WindowState = WindowState.Maximized;
            App.Profile.WindowSettings.IsFullscreen = true;
        }
    }

    public void OnToggleTitleBarCommand()
    {
        if (App.MainWindowView.WindowStyle == WindowStyle.SingleBorderWindow)
        {
            App.MainWindowView.WindowStyle = WindowStyle.None;
        }
        else
        {
            App.MainWindowView.WindowStyle = WindowStyle.SingleBorderWindow;
        }
    }

    public void OnToggleResizeBorderCommand()
    {
        if (App.MainWindowView.BorderThickness == new Thickness(3))
        {
            App.MainWindowView.BorderThickness = new Thickness(1);
        }
        else
        {
            App.MainWindowView.BorderThickness = new Thickness(3);
        }
    }

    public void OnToggleRecordingCommand()
    {
        if (IsRecording)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    public void OnCaptureScreenCommand()
    {
        Screenshot.Capture();
    }

    public async void OnLoadRecordingCommand()
    {
        if (App.MainWindowViewModel.IsRecording || !App.MainWindowViewModel.DisplayState.IsReady)
        {
            Message.Confirm("Cannot load replay while recording or display not ready");
            return;
        }
        OpenFileDialog openFileDialog = new OpenFileDialog
        {
            InitialDirectory = PathFinder.GetFolderPath("Recordings"),
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Select a JSON File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            SetDisplayReady(false);
            DisplayStatus = "Preparing Nav Data";
            string selectedFilePath = openFileDialog.FileName;
            PilotService.Stop();
            PilotService.Dispose();
            PilotService.Pilots.Clear();
            GraphicsEngine.RequestRender();
            PlaybackService.SetRecordingPath(selectedFilePath);
            JObject navData = (JObject)PlaybackService.ReplayJson["NavData"];
            NavData nasr = new NavData(null)
            {
                Date = (string)navData["Date"],
                ForceDownload = true,
                SwapNavDate = true,
                Delay = 250
            };
            await nasr.Run();
            RouteService = new((string)navData["Date"]);
            PlaybackService.Start();
            PlaybackService.Play();
            IsPlayback = true;
            SetTitle();
            ReplayControlVisibility = Visibility.Visible;
            ToolbarViewModel tbvm =  App.MainWindowView.ToolbarView.DataContext as ToolbarViewModel;
            tbvm.ExitReplayIsEnabled = true;
            ReplayControlsViewModel rcvm = App.MainWindowView.ReplayControlsView.DataContext as ReplayControlsViewModel;
            rcvm.MaximumSliderValue = PlaybackService.GetTotalTickCount();
            SetDisplayReady(true);
            return;
        }
        return;
    }

    public void OnExitRecordingCommand()
    {
        PilotService.Pilots.Clear();
        PlaybackService.Stop();
        IsPlayback = false;
        ToolbarViewModel tbvm = App.MainWindowView.ToolbarView.DataContext as ToolbarViewModel;
        tbvm.ExitReplayIsEnabled = false;
        ReplayControlVisibility = Visibility.Collapsed;
        SetTitle();
        PilotService.Start();
    }

    private void OnToggleTopDownCommand()
    {
        App.Profile.GeneralSettings.TopDown = !App.Profile.GeneralSettings.TopDown;
        GraphicsEngine.RequestRender();
    }

    private void OnSaveProfileCommand()
    {
        if (!DisplayState.IsReady) return;
        ProfileService.ConfirmSave();
    }

    private void OnSaveProfileAsCommand()
    {
        if (!DisplayState.IsReady) return;
        App.ViewManager.OpenSaveProfileAsView();
    }

    private void OnSwitchProfileCommand()
    {
        if (!DisplayState.IsReady) return;
        App.ViewManager.OpenLoadProfileView();
    }

    private void OnOpenGeneralSettingsCommand()
    {
        App.ViewManager.OpenGeneralSettingsView();
    }

    private void OnOpenAppearanceSettingsCommand()
    {
        App.ViewManager.OpenAppearanceSettingsView();
    }

    private void OnOpenMapsCommand()
    {
        App.ViewManager.OpenMapsView();
    }

    private void OnOpenPositionsCommand()
    {
        App.ViewManager.OpenPositionsView();
    }

    public void OnRewindCommand()
    {
        PlaybackService.Pause();
        App.MainWindowView.ReplayControlsView.PlayPauseButtonImage.Source = new BitmapImage(new Uri(PathFinder.GetFilePath("Resources/Images", "Play.png")));
        if (PlaybackService.Tick > 0) PlaybackService.Tick--;
        PlaybackService.Refresh();
    }

    public void OnFastForwardCommand()
    {
        PlaybackService.Pause();
        App.MainWindowView.ReplayControlsView.PlayPauseButtonImage.Source = new BitmapImage(new Uri(PathFinder.GetFilePath("Resources/Images", "Play.png")));
        if (PlaybackService.Tick < PlaybackService.GetTotalTickCount())
        {
            PlaybackService.Tick++;
            PlaybackService.Refresh();
        }
    }

    public void OnPlayPauseCommand()
    {
        if (PlaybackService.Paused)
        {
            PlaybackService.Paused = false;
            App.MainWindowView.ReplayControlsView.PlayPauseButtonImage.Source = new BitmapImage(new Uri(PathFinder.GetFilePath("Resources/Images", "Pause.png")));
            PlaybackService.Play();
        }
        else
        {
            PlaybackService.Paused = true;
            App.MainWindowView.ReplayControlsView.PlayPauseButtonImage.Source = new BitmapImage(new Uri(PathFinder.GetFilePath("Resources/Images", "Play.png")));
            PlaybackService.Pause();
        }
    }

    public void Dispose()
    {
        IsRecording = false;
        IsPlayback = false;
        PilotService.Dispose();
        DatablockService.Dispose();
        PlaybackService.Dispose();
        FindService.Dispose();
        EramFeatures = null;
        EramFeaturesCombined = null;
        ActiveEramMaps = null;
        StarsFeatures = null;
        RenderableFeatures = null;
        ActiveMaps = null;
    }
}
