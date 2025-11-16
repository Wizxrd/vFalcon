using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls;
using vFalcon.Services.Interfaces;

namespace vFalcon.Services.Service
{
    public class DragService : IDragService
    {
        private FrameworkElement _draggedElement;
        private FrameworkElement _boundaryElement;
        private Border _placeholder;
        private bool _isDragging;
        private Point _clickOffset;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ClipCursor(IntPtr lpRect); // release

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public void StartDrag(FrameworkElement elementToDrag, FrameworkElement boundary)
        {
            if (_isDragging || elementToDrag == null || !(elementToDrag.Parent is Canvas canvas))
                return;

            _draggedElement = elementToDrag;
            _boundaryElement = boundary;

            if (_placeholder == null)
            {
                _placeholder = new Border
                {
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.Transparent
                };
                canvas.Children.Add(_placeholder);
            }

            _placeholder.Width = elementToDrag.ActualWidth;
            _placeholder.Height = elementToDrag.ActualHeight;

            double left = Canvas.GetLeft(elementToDrag);
            double top = Canvas.GetTop(elementToDrag);
            Canvas.SetLeft(_placeholder, left);
            Canvas.SetTop(_placeholder, top);

            _placeholder.Visibility = Visibility.Visible;
            elementToDrag.Visibility = Visibility.Hidden;

            Point screenPoint = elementToDrag.PointToScreen(new Point(0, 0));
            SetCursorPos((int)screenPoint.X, (int)screenPoint.Y);

            _clickOffset = new Point(0, 0);

            var topLeft = _boundaryElement.PointToScreen(new Point(0, 0));
            var bottomRight = _boundaryElement.PointToScreen(
                new Point(_boundaryElement.ActualWidth, _boundaryElement.ActualHeight));

            RECT clipRect = new RECT
            {
                Left = (int)topLeft.X + 3,
                Top = (int)topLeft.Y + 3,
                Right = (int)bottomRight.X - 3,
                Bottom = (int)bottomRight.Y - 3
            };
            ClipCursor(ref clipRect);

            CompositionTarget.Rendering += OnRenderFrame;

            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(
                Application.Current.MainWindow,
                OnMouseDownDuringDrag);

            _isDragging = true;
        }

        public void StopDrag()
        {
            if (!_isDragging || _draggedElement == null || _placeholder == null)
                return;

            CompositionTarget.Rendering -= OnRenderFrame;

            Mouse.RemovePreviewMouseDownOutsideCapturedElementHandler(
                Application.Current.MainWindow,
                OnMouseDownDuringDrag);

            Canvas.SetLeft(_draggedElement, Canvas.GetLeft(_placeholder));
            Canvas.SetTop(_draggedElement, Canvas.GetTop(_placeholder));

            _draggedElement.Visibility = Visibility.Visible;
            _placeholder.Visibility = Visibility.Collapsed;

            ClipCursor(IntPtr.Zero); // release

            _draggedElement = null;
            _isDragging = false;
        }

        private void OnRenderFrame(object sender, EventArgs e)
        {
            if (!_isDragging || _placeholder?.Parent is not Canvas canvas || _boundaryElement == null)
                return;

            Point mousePos = Mouse.GetPosition(canvas);

            double maxX = _boundaryElement.ActualWidth - _placeholder.ActualWidth;
            double maxY = _boundaryElement.ActualHeight - _placeholder.ActualHeight;

            double newX = Math.Max(0, Math.Min(mousePos.X - _clickOffset.X, maxX));
            double newY = Math.Max(0, Math.Min(mousePos.Y - _clickOffset.Y, maxY));

            Canvas.SetLeft(_placeholder, newX);
            Canvas.SetTop(_placeholder, newY);
        }

        private void OnMouseDownDuringDrag(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                StopDrag();
                e.Handled = true;
            }
        }
    }
}
