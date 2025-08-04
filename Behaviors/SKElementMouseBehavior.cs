using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using SkiaSharp.Views.WPF;
using SkiaSharp;
using vFalcon.Helpers;

namespace vFalcon.Behaviors
{
    public class SKElementMouseBehavior : Behavior<SKElement>
    {
        public ICommand? MouseDownCommand
        {
            get => (ICommand)GetValue(MouseDownCommandProperty);
            set => SetValue(MouseDownCommandProperty, value);
        }

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(nameof(MouseDownCommand), typeof(ICommand), typeof(SKElementMouseBehavior));

        public ICommand? MouseMoveCommand
        {
            get => (ICommand)GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }

        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand), typeof(SKElementMouseBehavior));

        public ICommand? MouseUpCommand
        {
            get => (ICommand)GetValue(MouseUpCommandProperty);
            set => SetValue(MouseUpCommandProperty, value);
        }

        public static readonly DependencyProperty MouseUpCommandProperty =
            DependencyProperty.Register(nameof(MouseUpCommand), typeof(ICommand), typeof(SKElementMouseBehavior));

        public ICommand? MouseWheelCommand
        {
            get => (ICommand)GetValue(MouseWheelCommandProperty);
            set => SetValue(MouseWheelCommandProperty, value);
        }

        public static readonly DependencyProperty MouseWheelCommandProperty =
            DependencyProperty.Register(nameof(MouseWheelCommand), typeof(ICommand), typeof(SKElementMouseBehavior));

        protected override void OnAttached()
        {
            AssociatedObject.MouseDown += OnMouseDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseUp += OnMouseUp;
            AssociatedObject.MouseWheel += OnMouseWheel;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= OnMouseDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseUp -= OnMouseUp;
            AssociatedObject.MouseWheel -= OnMouseWheel;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            var args = new SKMouseEventArgs(e.ChangedButton.ToString(), new SKPoint((float)pos.X, (float)pos.Y));
            MouseDownCommand?.Execute(args);
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);

            string button = null;
            if (e.RightButton == MouseButtonState.Pressed)
                button = "Right";
            else if (e.LeftButton == MouseButtonState.Pressed)
                button = "Left";

            if (button != null)
            {
                var args = new SKMouseEventArgs(button, new SKPoint((float)pos.X, (float)pos.Y));
                MouseMoveCommand?.Execute(args);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            var args = new SKMouseEventArgs(e.ChangedButton.ToString(), new SKPoint((float)pos.X, (float)pos.Y));
            MouseUpCommand?.Execute(args);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var pos = e.GetPosition(AssociatedObject);
            var args = new SKMouseWheelEventArgs(e.Delta, new SKPoint((float)pos.X, (float)pos.Y));
            MouseWheelCommand?.Execute(args);
        }
    }
}
