using SkiaSharp;

namespace vFalcon.Helpers
{
    public class SKMouseEventArgs
    {
        public string Button { get; }
        public SKPoint Location { get; }

        public SKMouseEventArgs(string button, SKPoint location)
        {
            Button = button;
            Location = location;
        }
    }


    public class SKMouseWheelEventArgs
    {
        public int Delta { get; }
        public SKPoint Location { get; }

        public SKMouseWheelEventArgs(int delta, SKPoint location)
        {
            Delta = delta;
            Location = location;
        }
    }
}
