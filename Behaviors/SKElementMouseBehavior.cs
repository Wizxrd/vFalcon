using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using vFalcon.Helpers;

namespace vFalcon.Behaviors
{
    public class SKElementMouseBehavior : Behavior<SKElement>
    {
        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(nameof(MouseDownCommand), typeof(ICommand), typeof(SKElementMouseBehavior));

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(SKElementMouseBehavior));

        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.Register(nameof(MouseUpCommand), typeof(ICommand), typeof(SKElementMouseBehavior));

        public static readonly DependencyProperty MouseWheelCommandProperty =
            DependencyProperty.Register(nameof(MouseWheelCommand), typeof(ICommand), typeof(SKElementMouseBehavior));

        public ICommand? MouseDownCommand
        {
            get => (ICommand?)GetValue(MouseDownCommandProperty);
            set => SetValue(MouseDownCommandProperty, value);
        }

        public ICommand? MouseMoveCommand
        {
            get => (ICommand?)GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }

        public ICommand? MouseUpCommand
        {
            get => (ICommand?)GetValue(MouseUpCommandProperty);
            set => SetValue(MouseUpCommandProperty, value);
        }

        public ICommand? MouseWheelCommand
        {
            get => (ICommand?)GetValue(MouseWheelCommandProperty);
            set => SetValue(MouseWheelCommandProperty, value);
        }

        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += OnMouseDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseUp += OnMouseUp;
            AssociatedObject.MouseWheel += OnMouseWheel;
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= OnMouseDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseUp -= OnMouseUp;
            AssociatedObject.MouseWheel -= OnMouseWheel;
            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
            base.OnDetaching();
        }

        void OnMouseDown(object? sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
                Mouse.Capture(AssociatedObject);

            var pos = e.GetPosition(AssociatedObject);
            MouseDownCommand?.Execute(new SKMouseEventArgs(e.ChangedButton.ToString(), new SKPoint((float)pos.X, (float)pos.Y)));
        }

        void OnMouseMove(object? sender, MouseEventArgs e)
        {
            string? button =
                e.RightButton == MouseButtonState.Pressed ? "Right" :
                e.LeftButton == MouseButtonState.Pressed ? "Left" : null;

            if (button == null) return;

            var pos = e.GetPosition(AssociatedObject);
            MouseMoveCommand?.Execute(new SKMouseEventArgs(button, new SKPoint((float)pos.X, (float)pos.Y)));
        }

        void OnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right && Mouse.Captured == AssociatedObject)
                Mouse.Capture(null);

            var pos = e.GetPosition(AssociatedObject);
            MouseUpCommand?.Execute(new SKMouseEventArgs(e.ChangedButton.ToString(), new SKPoint((float)pos.X, (float)pos.Y)));
        }

        void OnMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            MouseWheelCommand?.Execute(new SKMouseWheelEventArgs(e.Delta, new SKPoint((float)pos.X, (float)pos.Y)));
        }

        void OnLostMouseCapture(object? sender, MouseEventArgs e)
        {
            var pos = Mouse.GetPosition(AssociatedObject);
            MouseUpCommand?.Execute(new SKMouseEventArgs("Right", new SKPoint((float)pos.X, (float)pos.Y)));
        }
    }
}
