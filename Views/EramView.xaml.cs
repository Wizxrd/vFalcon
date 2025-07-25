using AdonisUI.Controls;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vFalcon.Models;
using System.Runtime.InteropServices;
using vFalcon.Views;

namespace vFalcon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EramView : AdonisWindow
    {

        private bool isDraggingToolbar = false;
        private Point clickOffset;

        private FrameworkElement draggedElement; // original element (e.g., ToolbarPanel)
        private Border placeholder; // border shown during drag

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetCursorPos(int X, int Y);



        private Profile profile;

        public EramView(Profile profile)
        {
            InitializeComponent();
            ToolbarRegion.Content = new MasterToolbarView();
            var uri = new Uri($"pack://application:,,,/Resources/Cursors/Eram1.cur");
            this.Cursor = new Cursor(Application.GetResourceStream(uri).Stream);
        }

        private void GenericTearoff_Click(object sender, RoutedEventArgs e)
        {
            if (!isDraggingToolbar && sender is FrameworkElement button)
            {
                // Assume parent is what we want to drag (e.g., StackPanel inside a Canvas)
                FrameworkElement toDrag = button.Tag as FrameworkElement ?? button.Parent as FrameworkElement;
                if (toDrag == null) return;

                StartDraggingElement(toDrag);
            }
        }

        private void StartDraggingElement(FrameworkElement element)
        {
            if (element == null || !(element.Parent is Canvas canvas)) return;

            draggedElement = element;

            if (placeholder == null)
            {
                placeholder = new Border
                {
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.Transparent
                };
                canvas.Children.Add(placeholder);
            }

            placeholder.Width = element.ActualWidth;
            placeholder.Height = element.ActualHeight;

            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);
            Canvas.SetLeft(placeholder, left);
            Canvas.SetTop(placeholder, top);

            placeholder.Visibility = Visibility.Visible;
            element.Visibility = Visibility.Hidden;

            Point screenPoint = element.PointToScreen(new Point(0, 0));
            SetCursorPos((int)screenPoint.X, (int)screenPoint.Y);

            clickOffset = new Point(0, 0);

            // Clamp cursor to RadarVW bounds
            var topLeft = RadarVW.PointToScreen(new Point(0, 0));
            var bottomRight = RadarVW.PointToScreen(new Point(RadarVW.ActualWidth, RadarVW.ActualHeight));

            RECT clipRect = new RECT
            {
                Left = (int)topLeft.X+3,
                Top = (int)topLeft.Y+3,
                Right = (int)bottomRight.X-3,
                Bottom = (int)bottomRight.Y-3
            };
            ClipCursor(ref clipRect);

            isDraggingToolbar = true;
            CompositionTarget.Rendering += MoveElementWithMouse;
            Mouse.AddPreviewMouseDownHandler(this, OnMouseDownDuringDrag);
        }

        private void StopDraggingElement()
        {
            if (draggedElement == null || placeholder == null) return;

            CompositionTarget.Rendering -= MoveElementWithMouse;
            Mouse.RemovePreviewMouseDownHandler(this, OnMouseDownDuringDrag);
            isDraggingToolbar = false;

            Canvas.SetLeft(draggedElement, Canvas.GetLeft(placeholder));
            Canvas.SetTop(draggedElement, Canvas.GetTop(placeholder));

            draggedElement.Visibility = Visibility.Visible;
            placeholder.Visibility = Visibility.Collapsed;

            draggedElement = null;

            // Release cursor clipping
            ClipCursor(IntPtr.Zero);
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ClipCursor(IntPtr lpRect); // to release

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private void MoveElementWithMouse(object sender, EventArgs e)
        {
            if (placeholder?.Parent is Canvas canvas && RadarVW != null)
            {
                Point mousePos = Mouse.GetPosition(canvas);

                double maxX = RadarVW.ActualWidth - placeholder.ActualWidth;
                double maxY = RadarVW.ActualHeight - placeholder.ActualHeight;

                double newX = Math.Max(0, Math.Min(mousePos.X - clickOffset.X, maxX));
                double newY = Math.Max(0, Math.Min(mousePos.Y - clickOffset.Y, maxY));

                Canvas.SetLeft(placeholder, newX);
                Canvas.SetTop(placeholder, newY);
            }
        }

        private void OnMouseDownDuringDrag(object sender, MouseButtonEventArgs e)
        {
            if (isDraggingToolbar && e.LeftButton == MouseButtonState.Pressed)
            {
                StopDraggingElement();
                e.Handled = true;
            }
        }
    }
}