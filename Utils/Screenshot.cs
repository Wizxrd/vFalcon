using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace vFalcon.Utils;

public class Screenshot
{
    public static void Capture()
    {
        UIElement element = App.MainWindowView;
        var dpi = VisualTreeHelper.GetDpi(element);
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
}
