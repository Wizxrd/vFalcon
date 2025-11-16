using SkiaSharp;
using System.Windows.Controls;
using System.Windows.Media;
using vFalcon.Engines;

namespace vFalcon.UI.Views.Controls;

public partial class DisplayControlView : UserControl
{
    public SkiaEngine GraphicsEngine { get; }
    public DisplayControlView()
    {
        InitializeComponent();
        GraphicsEngine = new(this);
    }
}
