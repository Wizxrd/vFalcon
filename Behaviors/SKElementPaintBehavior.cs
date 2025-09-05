using System.Windows;
using Microsoft.Xaml.Behaviors;
using SkiaSharp.Views.WPF;
using SkiaSharp.Views.Desktop;
using System.Windows.Input;

namespace vFalcon.Behaviors
{
    public class SKElementPaintBehavior : Behavior<SKElement>
    {
        public static readonly DependencyProperty PaintCommandProperty = DependencyProperty.Register(nameof(PaintCommand), typeof(ICommand), typeof(SKElementPaintBehavior), new PropertyMetadata(null));

        public ICommand PaintCommand
        {
            get => (ICommand)GetValue(PaintCommandProperty);
            set => SetValue(PaintCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PaintSurface += OnPaintSurface;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PaintSurface -= OnPaintSurface;
            base.OnDetaching();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (PaintCommand?.CanExecute(e) == true) PaintCommand.Execute(e);
        }
    }
}
