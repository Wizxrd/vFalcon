using vFalcon.Engines;
using vFalcon.Utils;
namespace vFalcon.UI.ViewModels.Toolbar;

public class AppearanceSettingsViewModel
{
    public int Background
    {
        get => App.Profile.AppearanceSettings.Background;
        set
        {
            App.Profile.AppearanceSettings.Background = value;
            App.MainWindowViewModel.GraphicsEngine.BackgroundValue = value;
            App.MainWindowViewModel.GraphicsEngine.ScaleBackgroundByBacklight();
        }
    }

    public int Backlight
    {
        get => App.Profile.AppearanceSettings.Backlight;
        set
        {
            App.Profile.AppearanceSettings.Backlight = value;
            App.MainWindowViewModel.GraphicsEngine.BacklightValue = value;
            App.MainWindowViewModel.GraphicsEngine.ScaleBackgroundByBacklight();
        }
    }

    public int DatablockFontSize
    {
        get => App.Profile.AppearanceSettings.DatablockFontSize;
        set
        {
            App.Profile.AppearanceSettings.DatablockFontSize = value;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public int FullDatablockBrightness
    {
        get => App.Profile.AppearanceSettings.FullDatablockBrightness;
        set
        {
            App.Profile.AppearanceSettings.FullDatablockBrightness = value;
            Colors.EramFullDatablock = SkiaEngine.ScaleColor(Colors.Yellow, value);
            Colors.StarsFullDatablock = SkiaEngine.ScaleColor(Colors.White, value);
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public int LimitedDatablockBrightness
    {
        get => App.Profile.AppearanceSettings.LimitedDatablockBrightness;
        set
        {
            App.Profile.AppearanceSettings.LimitedDatablockBrightness = value;
            Colors.EramLimitedDatablock = SkiaEngine.ScaleColor(Colors.Yellow, value);
            Colors.StarsLimitedDatablock = SkiaEngine.ScaleColor(Colors.LimeGreen, value);
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public int MapBrightness
    {
        get => App.Profile.AppearanceSettings.MapBrightness;
        set
        {
            App.Profile.AppearanceSettings.MapBrightness = value;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public int HistoryBrightness
    {
        get => App.Profile.AppearanceSettings.HistoryBrightness;
        set
        {
            App.Profile.AppearanceSettings.HistoryBrightness = value;
            Colors.EramFullDatablockHistory = SkiaEngine.ScaleColor(Colors.Yellow, value);
            Colors.EramLimitedDatablockHistory = SkiaEngine.ScaleColor(Colors.Yellow, value);
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public int HistoryLength
    {
        get => App.Profile.AppearanceSettings.HistoryLength;
        set
        {
            App.Profile.AppearanceSettings.HistoryLength = value;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    public int VelocityVector
    {
        get => App.Profile.AppearanceSettings.VelocityVector;
        set
        {
            value = ToVelocityVector(value);
            App.Profile.AppearanceSettings.VelocityVector = value;
            App.MainWindowViewModel.GraphicsEngine.RequestRender();
        }
    }

    private static int ToVelocityVector(int v) => v <= 0 ? 0 : v <= 1 ? 1 : v <= 2 ? 2 : v <= 4 ? 4 : 8;
}
