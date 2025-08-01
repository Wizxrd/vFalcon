using OpenTK.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using vFalcon.Commands;
using vFalcon.Helpers;
using vFalcon.Models;
using vFalcon.Renderers;
using vFalcon.Services;

namespace vFalcon.ViewModels
{
    public class RadarViewModel : ViewModelBase
    {
        private EramViewModel eramViewModel;
        private VideoMap videoMap;


        public ICommand PaintSurfaceCommand { get; }

        public RadarViewModel(EramViewModel eramViewModel)
        {
            this.eramViewModel = eramViewModel;
            videoMap = new VideoMap();
            PaintSurfaceCommand = new RelayCommand(OnPaintCommand);
            CenterLat = eramViewModel.profile.DisplayWindowSettings != null
                ? (double)eramViewModel.profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Center"]["Lat"]
                : (double)eramViewModel.artcc.visibilityCenters[0]["lat"];
            CenterLon = eramViewModel.profile.DisplayWindowSettings != null
                ? (double)eramViewModel.profile.DisplayWindowSettings[0]["DisplaySettings"][0]["Center"]["Lon"]
                : (double)eramViewModel.artcc.visibilityCenters[0]["lon"];
        }
        //41.223661784766726, -80.9481329717042

        public Action? InvalidateCanvas;

        SKPoint PanOffset = new SKPoint();
        double Scale = 0.75;
        int Width;
        int Height;

        double CenterLat = 41.223661784766726;
        double CenterLon = -80.9481329717042;
        private bool isFirstRender = true;

        private void OnPaintCommand(object parameter)
        {
            if (parameter is SKPaintSurfaceEventArgs e)
            {
                Width = e.Info.Width;
                Height = e.Info.Height;
                var canvas = e.Surface.Canvas;
                canvas.Clear();

                if (isFirstRender)
                {
                    var pOffset = PanOffset;
                    VideoMap.CenterAtCoordinates(
                        Width,
                        Height,
                        Scale,
                        ref pOffset,
                        CenterLat,
                        CenterLon
                    );
                    PanOffset = pOffset;
                    isFirstRender = false;
                }

                var activeFeatures = new List<ProcessedFeature>();

                foreach (var kvp in eramViewModel.FacilityFeatures)
                {
                    int filterIndex = kvp.Key;

                    if (eramViewModel.ActiveFilters.Contains(filterIndex))
                    {
                        activeFeatures.AddRange(kvp.Value);
                    }
                }

                videoMap.Render(canvas, new System.Drawing.Size(Width, Height), Scale, PanOffset, activeFeatures);

            }
        }
    }
}
