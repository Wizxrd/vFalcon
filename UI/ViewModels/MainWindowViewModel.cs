using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using vFalcon.Engines;
using vFalcon.Models;
using vFalcon.Mvvm;
using vFalcon.Renderables;
using vFalcon.Renderables.Interfaces;
using vFalcon.Services;
using vFalcon.UI.ViewModels.Controls;
using vFalcon.UI.Views.Controls;
using vFalcon.UI.Views.Manager;
using vFalcon.Utils;
using Size = System.Drawing.Size;
namespace vFalcon.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private DisplayControlView DisplayControlView { get; set; }
    public Popup PilotContextPopup { get; set; }
    public PilotContextView PilotContextView { get; set; }
    public PilotContextViewModel PilotContextViewModel { get; set; }
    public RouteService RouteService { get; set; } = new(null);
    public ProfileService ProfileService = new();
    private string title = string.Empty;
    private string displayStatus = string.Empty;
    private string mouseCoordinates = string.Empty;
    private Visibility displayVisibility = Visibility.Collapsed;
    private Profile profile { get; set; } = App.Profile;
    private Artcc artcc { get; set; } = App.Artcc;

    public SkiaEngine GraphicsEngine { get; set; }
    public DisplayState DisplayState { get; set; }
    public PilotService PilotService { get; set; }
    public DatablockService DatablockService { get; set; }
    public SKPoint MousePosition { get; set; }
    public bool DisplayMouseCoordinates { get; set; } = false;

    public string Title
    {
        get => title;
        set
        {
            title = value;
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
    public ICommand ToggleTopDownCommand { get; set; }
    public ICommand SaveProfileCommand { get; set; }
    public ICommand SaveProfileAsCommand { get; set; }
    public ICommand SwitchProfileCommand { get; set; }
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
        GraphicsEngine.MouseDown += OnMouseDown;
        GraphicsEngine.MouseUp += OnMouseUp;
        GraphicsEngine.MouseMove += OnMouseMove;
        GraphicsEngine.MouseWheel += OnMouseWheel;
        GraphicsEngine.PaintSurface += PaintSurface;

        DisplayState = new();
        PilotService = new();
        DatablockService = new();
        PilotService.DisplayState = DisplayState;

        ClearCommand = new RelayCommand(OnClearCommand);
        ToggleTopDownCommand = new RelayCommand(OnToggleTopDownCommand);
        SaveProfileCommand = new RelayCommand(OnSaveProfileCommand);
        SaveProfileAsCommand = new RelayCommand(OnSaveProfileAsCommand);
        SwitchProfileCommand = new RelayCommand(OnSwitchProfileCommand);
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
        await InitFeatures();
        SetDisplayState();
        SetActiveMaps();
        StartServices();
    }

    public void SetDisplayState()
    {
        if (profile.Center == null)
        {
            double lat = (double)artcc.visibilityCenters[0]["lat"];
            double lon = (double)artcc.visibilityCenters[0]["lon"];
            DisplayState.Center.Lat = lat;
            DisplayState.Center.Lon = lon;
        }
        else
        {
            DisplayState.Center = profile.Center;
        }
        ZoomLevels = Zoom.BuildLevels();
        ScaleMap = Zoom.BuildScale(DisplayState, ZoomLevels);
        DisplayState.Scale = ScaleMap[profile.ZoomIndex];
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
        Colors.EramFullDatablock = SkiaEngine.ScaleColor(Colors.Yellow, profile.AppearanceSettings.FullDatablockBrightness);
        Colors.StarsFullDatablock = SkiaEngine.ScaleColor(Colors.White, profile.AppearanceSettings.FullDatablockBrightness);
        Colors.EramLimitedDatablock = SkiaEngine.ScaleColor(Colors.Yellow, profile.AppearanceSettings.LimitedDatablockBrightness);
        Colors.StarsLimitedDatablock = SkiaEngine.ScaleColor(Colors.LimeGreen, profile.AppearanceSettings.LimitedDatablockBrightness);
        Colors.EramFullDatablockHistory = SkiaEngine.ScaleColor(Colors.Yellow, profile.AppearanceSettings.HistoryBrightness);
        Colors.EramLimitedDatablockHistory = SkiaEngine.ScaleColor(Colors.Yellow, profile.AppearanceSettings.HistoryBrightness);
    }

    public void SetTitle()
    {
        Title = $"vFalcon : {profile.ArtccId}";
    }

    public void SetBackground()
    {
        GraphicsEngine.BackgroundValue = profile.AppearanceSettings.Background;
        GraphicsEngine.BacklightValue = profile.AppearanceSettings.Backlight;
        GraphicsEngine.ScaleBackgroundByBacklight();
    }

    public void SetSurveillanceAoi()
    {
        string json = File.ReadAllText(Path.Combine(PathFinder.GetAppDirectory(), "SurveillanceAois.geojson"));
        Surveillance surveillance = JsonConvert.DeserializeObject<Surveillance>(json);
        foreach (JObject feature in surveillance.features)
        {
            var id = ((JObject)feature["properties"])?.GetValue("id", StringComparison.OrdinalIgnoreCase)?.ToString();
            if (string.Equals(id, profile.ArtccId))
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

    public async Task InitFeatures()
    {
        EramFeatures.Clear();
        EramFeaturesCombined.Clear();
        ActiveEramMaps.Clear();
        DisplayState.IsReady = false;
        DisplayVisibility = Visibility.Collapsed;
        DisplayStatus = "Preparing Video Maps";
        foreach (JObject geoMap in Models.VideoMap.GetGeoMaps()) ActiveEramMaps[(string)geoMap["name"]] = new HashSet<string>();
        if (App.Profile.MapSettings.GeoMap == string.Empty) App.Profile.MapSettings.GeoMap = (string)artcc.facility["eramConfiguration"]["geoMaps"][0]["name"];
        JObject defaultStarsFacility = (JObject)artcc.facility["childFacilities"][0];
        if (App.Profile.MapSettings.Facility == string.Empty) App.Profile.MapSettings.Facility = $"{defaultStarsFacility["id"]} - {defaultStarsFacility["name"]}";
        if (profile.VideoMapPreProcess)
        {
            Logger.Debug("InitFeatures", "Video Map Pre-Processing Enabled");
            StarsFeatures = await GeoJson.GetAllStarsFeatures();
            EramFeaturesCombined = await GeoJson.GetAllEramFeatures();
        }
        else
        {
            EramFeatures = await GeoJson.GetEramFacilityFeatures(artcc, JArray.FromObject(VideoMap.GetMapsetVideoMapIds(App.Profile.MapSettings.GeoMap)));
            StarsFeatures = await GeoJson.GetStarsFacilityFeatures(artcc, VideoMap.GetFaciltiyVideoMaps(App.Profile.MapSettings.Facility.Substring(0, 3)));
        }
        DisplayStatus = string.Empty;
        DisplayState.IsReady = true;
        DisplayVisibility = Visibility.Visible;
        SetRenderableFeatures();
    }

    public async void ReloadEramFeatures(bool reloadEramFeatures = false)
    {;
        if (!App.Profile.VideoMapPreProcess && reloadEramFeatures)
        {
            EramFeatures.Clear();
            DisplayState.IsReady = false;
            DisplayVisibility = Visibility.Collapsed;
            DisplayStatus = "Preparing Video Maps";
            EramFeatures = await GeoJson.GetEramFacilityFeatures(artcc, JArray.FromObject(VideoMap.GetMapsetVideoMapIds(App.Profile.MapSettings.GeoMap)));
            DisplayStatus = string.Empty;
            DisplayState.IsReady = true;
            DisplayVisibility = Visibility.Visible;
        }
        else if (App.Profile.VideoMapPreProcess)
        {
            EramFeatures = EramFeaturesCombined[profile.MapSettings.GeoMap];
        }
        SetRenderableFeatures();
        GraphicsEngine.RequestRender();
    }

    public async void ReloadStarsFeatures(string facilityId)
    {
        if (!App.Profile.VideoMapPreProcess)
        {
            StarsFeatures.Clear();
            DisplayState.IsReady = false;
            DisplayVisibility = Visibility.Collapsed;
            DisplayStatus = "Preparing Video Maps";
            StarsFeatures = await GeoJson.GetStarsFacilityFeatures(artcc, VideoMap.GetFaciltiyVideoMaps(facilityId));
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
                        if (isTdmOnly && !profile.TopDown)
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

    private void OnMouseDown(object sender, SKPoint point, MouseButton button)
    {
        if (button == MouseButton.Right)
        {
            OpenPilotContextView(sender, point);
            if (MousePosition == point) return;
            MousePosition = point;
            DisplayState.IsPanning = true;
            GraphicsEngine.StartPan();
        }
        else if (button == MouseButton.Middle)
        {
            Pilot? clickedOnPilot = PilotService.IsPilotClickedOn(point);
            if (clickedOnPilot != null)
            {
                clickedOnPilot.ForcedFullDatablock = !clickedOnPilot.ForcedFullDatablock;
                clickedOnPilot.FullDatablockEnabled = !clickedOnPilot.FullDatablockEnabled;
                GraphicsEngine.RequestRender();
            }
        }
    }

    private void OnMouseMove(object sender, SKPoint point)
    {
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
        if (button == MouseButton.Right)
        {
            Coordinate newCenter = ScreenMap.ScreenToCoordinate(new System.Drawing.Size(DisplayState.Width, DisplayState.Height), DisplayState.Scale, DisplayState.PanOffset, new SKPoint(DisplayState.Width / 2f, DisplayState.Height / 2f));
            DisplayState.Center.Lat = newCenter.Lat;
            DisplayState.Center.Lon = newCenter.Lon;
            MousePosition = new SKPoint();
            DisplayState.IsPanning = false;
            GraphicsEngine.StopPan();
        }
    }

    private void OnMouseWheel(object sender, SKPoint point, int delta)
    {
        if (!DisplayState.IsReady) return;
        PilotContextPopup.IsOpen = false;
        int direction = delta > 0 ? -1 : 1;
        int newIndex = Math.Clamp(profile.ZoomIndex + delta, 0, ZoomLevels.Count - 1);
        if (newIndex == profile.ZoomIndex) return;

        SKPoint referencePoint = DisplayState.ZoomOnMouse ? point : new SKPoint(DisplayState.Width / 2f, DisplayState.Height / 2f);

        var before = new SKPoint(
            (referencePoint.X - DisplayState.PanOffset.X - DisplayState.Width / 2f) / (float)DisplayState.Scale,
            (referencePoint.Y - DisplayState.PanOffset.Y - DisplayState.Height / 2f) / (float)DisplayState.Scale
        );

        profile.ZoomIndex = newIndex;

        DisplayState.Scale = ScaleMap[profile.ZoomIndex];

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
            var videoMapRenderables = Renderables.Renderables.FromFeatures(DisplayState, RenderableFeatures);
            GraphicsEngine.Renderables.AddRange(videoMapRenderables);
        }

        if (PilotService.Pilots.Count > 0)
        {
            var pilotRenderables = Renderables.Renderables.FromPilots(DisplayState, PilotService.Pilots);
            GraphicsEngine.Renderables.AddRange(pilotRenderables);
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
        }
        PilotContextPopup.IsOpen = false;
        GraphicsEngine.RequestRender();
    }

    private void OnToggleTopDownCommand()
    {
        profile.TopDown = !profile.TopDown;
        GraphicsEngine.RequestRender();
    }

    private void OnSaveProfileCommand()
    {
        ProfileService.ConfirmSave();
    }

    private void OnSaveProfileAsCommand()
    {
        App.ViewManager.OpenSaveProfileAsView();
    }

    private void OnSwitchProfileCommand()
    {
        App.ViewManager.OpenLoadProfileView();
    }
}
